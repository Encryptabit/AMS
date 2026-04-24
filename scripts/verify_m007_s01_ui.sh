#!/usr/bin/env bash
# M007/S01/T05 UI verification harness.
#
# Starts the Ams.Workstation.Server locally, drives Chromium via Playwright to
# exercise mobile-first layout + theme-toggle behavior on migrated surfaces,
# and emits a pages/console/timeline/summary evidence bundle under
# .artifacts/browser/<timestamp>-m007-s01-uat/. Exits non-zero if any assertion
# fails so CI/automation can trust the verdict.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

PROJECT_PATH="host/Ams.Workstation.Server/Ams.Workstation.Server.csproj"
PLAYWRIGHT_VERSION="${AMS_PLAYWRIGHT_VERSION:-1.59.1}"
HOST="${AMS_VERIFY_HOST:-127.0.0.1}"
TIMEOUT_START_SEC="${AMS_VERIFY_STARTUP_TIMEOUT:-90}"
KEEP_ARTIFACTS_ON_FAIL="${AMS_VERIFY_KEEP_ARTIFACTS:-1}"
# Playwright is installed into this cache dir on first run and reused thereafter.
# We drop the driver into the same dir so its ESM `import "playwright"` resolves
# against the adjacent node_modules/ without needing NODE_PATH or global installs.
PW_CACHE_DIR="${AMS_PW_CACHE_DIR:-$REPO_ROOT/.artifacts/browser/.pwcache/${PLAYWRIGHT_VERSION}}"

# Keep Playwright off the network for the browser download — we rely on the
# cached Chromium installed at ~/.cache/ms-playwright. Skipping the post-install
# download avoids flaky runs and surfaces missing binaries clearly.
export PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD="${PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD:-1}"

timestamp() {
    date -u +"%Y-%m-%dT%H-%M-%S-%3NZ"
}

TS="$(timestamp)"
ARTIFACTS_DIR="$REPO_ROOT/.artifacts/browser/${TS}-m007-s01-uat"
mkdir -p "$ARTIFACTS_DIR"
mkdir -p "$PW_CACHE_DIR"

SERVER_LOG="$ARTIFACTS_DIR/server.log"
DRIVER_OUT="$ARTIFACTS_DIR/driver.stdout.log"
DRIVER_ERR="$ARTIFACTS_DIR/driver.stderr.log"
# Driver lives inside PW_CACHE_DIR so node's ESM resolver finds `playwright` in
# the sibling node_modules/. A pointer file in the per-run artifacts dir keeps
# the evidence bundle self-describing.
DRIVER_JS="$PW_CACHE_DIR/driver.mjs"
DRIVER_POINTER="$ARTIFACTS_DIR/driver.mjs.path"

SERVER_PID=""

log() {
    printf '[verify_m007_s01_ui] %s\n' "$*" >&2
}

cleanup() {
    local exit_code=$?
    if [[ -n "$SERVER_PID" ]] && kill -0 "$SERVER_PID" 2>/dev/null; then
        log "Stopping workstation server (pid=$SERVER_PID)"
        kill "$SERVER_PID" 2>/dev/null || true
        # Give it a beat to shut down cleanly, then SIGKILL if still alive.
        for _ in 1 2 3 4 5; do
            if ! kill -0 "$SERVER_PID" 2>/dev/null; then break; fi
            sleep 0.3
        done
        if kill -0 "$SERVER_PID" 2>/dev/null; then
            kill -9 "$SERVER_PID" 2>/dev/null || true
        fi
    fi
    if [[ $exit_code -ne 0 && "$KEEP_ARTIFACTS_ON_FAIL" == "0" ]]; then
        rm -rf "$ARTIFACTS_DIR"
    else
        log "Artifacts: $ARTIFACTS_DIR"
    fi
    exit "$exit_code"
}
trap cleanup EXIT INT TERM

require_bin() {
    if ! command -v "$1" >/dev/null 2>&1; then
        log "Required binary not on PATH: $1"
        exit 127
    fi
}

require_bin dotnet
require_bin node
require_bin npx
require_bin curl

pick_port() {
    # Ask the kernel for a free TCP port; Playwright driver + server both need to
    # agree, so we allocate once and pin both sides to it.
    python3 - <<'PY' 2>/dev/null || node -e 'const n=require("net");const s=n.createServer();s.listen(0,()=>{const p=s.address().port;s.close(()=>console.log(p))})'
import socket
s = socket.socket()
s.bind(("127.0.0.1", 0))
print(s.getsockname()[1])
s.close()
PY
}

PORT="${AMS_VERIFY_PORT:-$(pick_port)}"
BASE_URL="http://${HOST}:${PORT}"

log "Artifacts dir: $ARTIFACTS_DIR"
log "Base URL: $BASE_URL"

log "Restoring + building $PROJECT_PATH (Debug)"
if ! dotnet build "$PROJECT_PATH" -c Debug --nologo -v minimal >"$ARTIFACTS_DIR/build.log" 2>&1; then
    log "Workstation build failed; see $ARTIFACTS_DIR/build.log"
    tail -n 60 "$ARTIFACTS_DIR/build.log" >&2 || true
    exit 1
fi

log "Starting workstation server on $BASE_URL"
# Never background with bare '&' — stdout/stderr must be redirected or the
# parent shell hangs waiting for the streams to close.
#
# --no-launch-profile is required because launchSettings.json pins the server
# to http://0.0.0.0:8081; without it, dotnet run silently ignores ASPNETCORE_URLS
# and the driver talks to the wrong port.
ASPNETCORE_URLS="$BASE_URL" \
ASPNETCORE_ENVIRONMENT=Development \
DOTNET_ENVIRONMENT=Development \
    dotnet run --no-build --no-launch-profile \
    --project "$PROJECT_PATH" -c Debug \
    --urls "$BASE_URL" \
    >"$SERVER_LOG" 2>&1 &
SERVER_PID=$!
log "Server pid: $SERVER_PID"

deadline=$(( $(date +%s) + TIMEOUT_START_SEC ))
started=0
while (( $(date +%s) < deadline )); do
    if ! kill -0 "$SERVER_PID" 2>/dev/null; then
        log "Server process exited before readiness; see $SERVER_LOG"
        tail -n 80 "$SERVER_LOG" >&2 || true
        exit 1
    fi
    if curl -s -o /dev/null --max-time 2 -w '%{http_code}' "$BASE_URL/" 2>/dev/null | grep -qE '^(200|3..)$'; then
        started=1
        break
    fi
    sleep 0.5
done

if (( started == 0 )); then
    log "Server did not become reachable within ${TIMEOUT_START_SEC}s"
    tail -n 80 "$SERVER_LOG" >&2 || true
    exit 1
fi

log "Server reachable; running Playwright driver"

# Install playwright locally if we do not already have it for this version.
if [[ ! -d "$PW_CACHE_DIR/node_modules/playwright" ]]; then
    log "Installing playwright@${PLAYWRIGHT_VERSION} into $PW_CACHE_DIR (first run)"
    ( cd "$PW_CACHE_DIR" && \
        printf '{"name":"ams-s01-pw-driver","private":true,"version":"0.0.0","type":"module"}\n' >package.json && \
        npm install --silent --no-audit --no-fund "playwright@${PLAYWRIGHT_VERSION}" >"$ARTIFACTS_DIR/pw-install.log" 2>&1 )
fi

printf '%s\n' "$DRIVER_JS" >"$DRIVER_POINTER"

cat >"$DRIVER_JS" <<'JS'
// Playwright UAT driver for M007/S01/T05.
//
// Exercises: mobile-first layout at 360/768/1280, HeaderControls presence,
// theme toggle state + computed style delta, and unmigrated routes sanity.
// Emits pages.json, console.json, timeline.json, summary.json.
import { chromium } from "playwright";
import fs from "node:fs/promises";
import path from "node:path";

const BASE_URL = process.env.AMS_VERIFY_BASE_URL;
const ARTIFACTS_DIR = process.env.AMS_VERIFY_ARTIFACTS_DIR;
if (!BASE_URL || !ARTIFACTS_DIR) {
    console.error("Missing AMS_VERIFY_BASE_URL or AMS_VERIFY_ARTIFACTS_DIR");
    process.exit(2);
}

const VIEWPORTS = [
    { name: "mobile-360", width: 360, height: 780 },
    { name: "tablet-768", width: 768, height: 1024 },
    { name: "desktop-1280", width: 1280, height: 900 },
];

/** @type {Array<{name:string, status:"PASS"|"FAIL", detail?:string, data?:any}>} */
const checks = [];
/** @type {Array<{t:number, kind:string, detail:string}>} */
const timeline = [];
/** @type {Array<{url:string, viewport?:string, messages:Array<{type:string,text:string}>, errors:Array<string>, failedRequests:Array<{url:string,status:number}>}>} */
const pages = [];
const consoleAll = [];

const t0 = Date.now();
function mark(kind, detail) {
    timeline.push({ t: Date.now() - t0, kind, detail });
}
function record(name, status, detail, data) {
    checks.push({ name, status, detail, data });
    mark("check", `${status} ${name}${detail ? " :: " + detail : ""}`);
}

async function capturePage(page, context, viewport) {
    const messages = [];
    const errors = [];
    const failedRequests = [];
    const handlers = [];

    const onConsole = (msg) => {
        const entry = { type: msg.type(), text: msg.text(), url: page.url() };
        messages.push(entry);
        consoleAll.push({ ...entry, context, viewport: viewport?.name });
    };
    const onPageError = (err) => {
        errors.push(err.stack || err.message || String(err));
    };
    const onRequestFailed = (req) => {
        failedRequests.push({ url: req.url(), status: 0, error: req.failure()?.errorText });
    };
    const onResponse = (res) => {
        if (res.status() >= 400) failedRequests.push({ url: res.url(), status: res.status() });
    };

    page.on("console", onConsole);
    page.on("pageerror", onPageError);
    page.on("requestfailed", onRequestFailed);
    page.on("response", onResponse);

    handlers.push(() => page.off("console", onConsole));
    handlers.push(() => page.off("pageerror", onPageError));
    handlers.push(() => page.off("requestfailed", onRequestFailed));
    handlers.push(() => page.off("response", onResponse));

    return {
        teardown() { for (const h of handlers) h(); },
        messages,
        errors,
        failedRequests,
    };
}

async function runHomeViewportChecks(page) {
    for (const vp of VIEWPORTS) {
        mark("viewport", `Home @ ${vp.name}`);
        await page.setViewportSize({ width: vp.width, height: vp.height });
        const resp = await page.goto(`${BASE_URL}/`, { waitUntil: "networkidle" });
        if (!resp || !resp.ok()) {
            record(`home.load.${vp.name}`, "FAIL", `status=${resp?.status()}`);
            continue;
        }
        record(`home.load.${vp.name}`, "PASS", `status=${resp.status()}`);

        const measurements = await page.evaluate(() => {
            const docEl = document.documentElement;
            const body = document.body;
            const themeAttr = docEl.getAttribute("data-ams-theme");
            const primaryBg = getComputedStyle(body).backgroundColor;
            const primaryFg = getComputedStyle(body).color;
            const primaryButtons = Array.from(
                document.querySelectorAll(".ams-btn.ams-btn--md, .ams-btn.ams-btn--lg"),
            ).map((el) => ({
                tag: el.tagName.toLowerCase(),
                classes: el.getAttribute("class"),
                offsetHeight: el.offsetHeight,
                offsetWidth: el.offsetWidth,
                text: (el.textContent || "").trim().slice(0, 40),
            }));
            const headerToggle = document.querySelector('[data-ams-header-control="theme-toggle"]');
            const headerChapter = document.querySelector('[data-ams-header-control="chapter"]');
            const headerWorkspaces = document.querySelector('[data-ams-header-control="saved-workspaces"]');
            // The `/` migrated surface lives inside `.workstation-content` (main
            // scroll region). We measure overflow there rather than on <html>
            // because MainLayout legacy chrome (header + module rail) is outside
            // S01's scope — its overflow behavior is deferred to its migration
            // slice. The migrated Home content must fit within its scroll area.
            const workstationContent = document.querySelector(".workstation-content");
            const homeContainer = document.querySelector(".home-container");
            return {
                scrollWidth: docEl.scrollWidth,
                clientWidth: docEl.clientWidth,
                innerWidth: window.innerWidth,
                workstationContent: workstationContent ? {
                    scrollWidth: workstationContent.scrollWidth,
                    clientWidth: workstationContent.clientWidth,
                } : null,
                homeContainer: homeContainer ? {
                    scrollWidth: homeContainer.scrollWidth,
                    clientWidth: homeContainer.clientWidth,
                    offsetWidth: homeContainer.offsetWidth,
                } : null,
                themeAttr,
                primaryBg,
                primaryFg,
                primaryButtons,
                headerControlsVisible: {
                    themeToggle: !!(headerToggle && headerToggle.offsetParent !== null),
                    workspaces: !!(headerWorkspaces && headerWorkspaces.offsetParent !== null),
                    chapter: !!headerChapter,
                },
            };
        });

        // No horizontal overflow on the migrated content region. Allow 1px for
        // sub-pixel rounding. Legacy chrome (workstation-header, module rail)
        // is out of scope until its migration slice lands.
        const region = measurements.workstationContent;
        if (!region) {
            record(`home.no_horizontal_overflow.${vp.name}`, "FAIL",
                "`.workstation-content` not found — layout regression?");
        } else {
            const overflow = region.scrollWidth - region.clientWidth;
            if (overflow <= 1) {
                record(`home.no_horizontal_overflow.${vp.name}`, "PASS",
                    `.workstation-content scrollWidth=${region.scrollWidth} clientWidth=${region.clientWidth}`,
                    measurements);
            } else {
                record(`home.no_horizontal_overflow.${vp.name}`, "FAIL",
                    `.workstation-content scrollWidth=${region.scrollWidth} > clientWidth=${region.clientWidth} (overflow=${overflow})`,
                    measurements);
            }
        }

        // Primary touch targets >=44px. Home renders md-size buttons + md-size
        // anchor quick links; sm-sized header nav is intentionally compact.
        const smallButtons = measurements.primaryButtons.filter((b) => b.offsetHeight < 44);
        if (measurements.primaryButtons.length === 0) {
            record(`home.touch_targets.${vp.name}`, "FAIL", "no md/lg .ams-btn elements found on Home");
        } else if (smallButtons.length === 0) {
            record(`home.touch_targets.${vp.name}`, "PASS",
                `${measurements.primaryButtons.length} button(s) all >=44px high`,
                measurements.primaryButtons);
        } else {
            record(`home.touch_targets.${vp.name}`, "FAIL",
                `${smallButtons.length} md/lg button(s) under 44px`,
                smallButtons);
        }

        // HeaderControls operable at every viewport — theme toggle + workspace
        // chooser must be in the DOM and laid out.
        if (measurements.headerControlsVisible.themeToggle && measurements.headerControlsVisible.workspaces) {
            record(`header.visible.${vp.name}`, "PASS", "theme toggle + workspaces select present");
        } else {
            record(`header.visible.${vp.name}`, "FAIL",
                `themeToggle=${measurements.headerControlsVisible.themeToggle} workspaces=${measurements.headerControlsVisible.workspaces}`,
                measurements.headerControlsVisible);
        }

        if (measurements.themeAttr === "dark") {
            record(`theme.default.${vp.name}`, "PASS", `data-ams-theme=${measurements.themeAttr}`);
        } else {
            record(`theme.default.${vp.name}`, "FAIL", `data-ams-theme=${measurements.themeAttr} (expected 'dark')`);
        }

        pages.push({
            url: page.url(),
            viewport: vp.name,
            context: "home-viewport",
            measurements,
        });
    }
}

async function runThemeToggleCheck(page) {
    mark("theme", "starting theme toggle check");
    await page.setViewportSize({ width: 1280, height: 900 });
    const resp = await page.goto(`${BASE_URL}/`, { waitUntil: "networkidle" });
    if (!resp || !resp.ok()) {
        record("theme.toggle.navigate", "FAIL", `status=${resp?.status()}`);
        return;
    }

    // Wait for Blazor Server to have hydrated the theme toggle button so
    // @onclick is wired before we click.
    await page.waitForSelector('[data-ams-header-control="theme-toggle"]', { state: "attached", timeout: 15000 });
    // Blazor Server binds handlers only after the SignalR circuit is live.
    // A short settle lets the circuit come up; an alternative would be to
    // poll for `Blazor` on window, but that symbol is private in release builds.
    await page.waitForTimeout(1500);

    const before = await page.evaluate(() => ({
        theme: document.documentElement.getAttribute("data-ams-theme"),
        bg: getComputedStyle(document.body).backgroundColor,
        fg: getComputedStyle(document.body).color,
    }));

    await page.click('[data-ams-header-control="theme-toggle"]');
    // Wait for the attribute flip rather than a fixed timeout.
    await page.waitForFunction(
        (prev) => document.documentElement.getAttribute("data-ams-theme") !== prev,
        before.theme,
        { timeout: 5000 },
    ).catch(() => { /* fall through; the following evaluate captures the state */ });

    const afterFirst = await page.evaluate(() => ({
        theme: document.documentElement.getAttribute("data-ams-theme"),
        bg: getComputedStyle(document.body).backgroundColor,
        fg: getComputedStyle(document.body).color,
    }));

    if (before.theme === "dark" && afterFirst.theme === "light") {
        record("theme.toggle.attribute_flip", "PASS", `dark -> light`);
    } else {
        record("theme.toggle.attribute_flip", "FAIL", `before=${before.theme} after=${afterFirst.theme}`);
    }

    if (before.bg !== afterFirst.bg) {
        record("theme.toggle.bg_changed", "PASS", `bg: ${before.bg} -> ${afterFirst.bg}`);
    } else {
        record("theme.toggle.bg_changed", "FAIL", `body background did not change (${before.bg})`);
    }

    // Flip back to dark so subsequent checks run under the configured default.
    await page.click('[data-ams-header-control="theme-toggle"]');
    await page.waitForFunction(
        (prev) => document.documentElement.getAttribute("data-ams-theme") !== prev,
        afterFirst.theme,
        { timeout: 5000 },
    ).catch(() => { });

    const afterReturn = await page.evaluate(() => ({
        theme: document.documentElement.getAttribute("data-ams-theme"),
        bg: getComputedStyle(document.body).backgroundColor,
    }));
    if (afterReturn.theme === "dark") {
        record("theme.toggle.return_to_dark", "PASS", `light -> dark`);
    } else {
        record("theme.toggle.return_to_dark", "FAIL", `ended at ${afterReturn.theme}`);
    }

    pages.push({
        url: page.url(),
        context: "theme-toggle",
        before,
        afterFirst,
        afterReturn,
    });
}

async function runUnmigratedRouteChecks(page) {
    // Unmigrated routes are still Bit-powered. We only assert they load without
    // throwing and still resolve the dark theme attribute — behavior parity
    // with Bit controls is out of scope for S01.
    const routes = ["/prep", "/proof", "/polish"];
    await page.setViewportSize({ width: 1280, height: 900 });
    for (const route of routes) {
        mark("route", `visit ${route}`);
        const errors = [];
        const onErr = (e) => errors.push(e.stack || e.message || String(e));
        page.on("pageerror", onErr);
        let resp;
        try {
            resp = await page.goto(`${BASE_URL}${route}`, { waitUntil: "domcontentloaded", timeout: 20000 });
        } catch (err) {
            record(`route.load.${route}`, "FAIL", `navigation error: ${err?.message || err}`);
            page.off("pageerror", onErr);
            continue;
        }

        const status = resp?.status() ?? 0;
        const themeAttr = await page.evaluate(() => document.documentElement.getAttribute("data-ams-theme"));
        // Give the page a beat to settle, then unhook the per-route error listener.
        await page.waitForTimeout(750);
        page.off("pageerror", onErr);

        if (status >= 200 && status < 400 && errors.length === 0) {
            record(`route.load.${route}`, "PASS", `status=${status} theme=${themeAttr}`);
        } else {
            record(`route.load.${route}`, "FAIL",
                `status=${status} jsErrors=${errors.length} theme=${themeAttr}`,
                { errors });
        }

        if (themeAttr === "dark") {
            record(`route.theme.${route}`, "PASS", `data-ams-theme=dark preserved on Bit-powered route`);
        } else {
            record(`route.theme.${route}`, "FAIL", `expected dark, got ${themeAttr}`);
        }

        pages.push({
            url: page.url(),
            route,
            context: "unmigrated-route",
            status,
            themeAttr,
            errors,
        });
    }
}

async function assertNoMigratedConsoleErrors() {
    // Treat console 'error' entries from migrated contexts as hard failures.
    // SignalR/HTTP warnings and Bit-routed console noise on unmigrated routes
    // are out of scope for this slice.
    const migratedContexts = new Set(["home-viewport", "theme-toggle"]);
    const bad = consoleAll.filter((m) => migratedContexts.has(m.context) && m.type === "error");
    if (bad.length === 0) {
        record("console.no_errors_migrated", "PASS", "no console.error on Home / theme-toggle flows");
    } else {
        record("console.no_errors_migrated", "FAIL", `${bad.length} console.error entries`, bad);
    }
}

async function main() {
    mark("driver", "launch chromium");
    // Prefer an explicit Chromium binary when provided so the harness works in
    // offline/air-gapped environments where Playwright's own browser cache may
    // not match the installed package version.
    const launchOpts = { args: ["--no-sandbox", "--disable-dev-shm-usage"] };
    if (process.env.AMS_CHROMIUM_EXECUTABLE) {
        launchOpts.executablePath = process.env.AMS_CHROMIUM_EXECUTABLE;
    }
    const browser = await chromium.launch(launchOpts);
    const context = await browser.newContext();
    const page = await context.newPage();

    // Home viewport matrix + default theme check.
    const homeCap = await capturePage(page, "home-viewport", null);
    try {
        await runHomeViewportChecks(page);
    } finally {
        homeCap.teardown();
    }

    // Theme toggle.
    const themeCap = await capturePage(page, "theme-toggle", null);
    try {
        await runThemeToggleCheck(page);
    } finally {
        themeCap.teardown();
    }

    // Unmigrated routes.
    const routeCap = await capturePage(page, "unmigrated-route", null);
    try {
        await runUnmigratedRouteChecks(page);
    } finally {
        routeCap.teardown();
    }

    await assertNoMigratedConsoleErrors();

    await browser.close();
    mark("driver", "closed chromium");
}

const runStartedAt = new Date().toISOString();

try {
    await main();
} catch (err) {
    record("driver.uncaught", "FAIL", err?.stack || err?.message || String(err));
}

const passCount = checks.filter((c) => c.status === "PASS").length;
const failCount = checks.filter((c) => c.status === "FAIL").length;
const verdict = failCount === 0 ? "PASS" : "FAIL";

const summary = {
    slice: "M007/S01",
    task: "T05",
    verdict,
    startedAt: runStartedAt,
    finishedAt: new Date().toISOString(),
    baseUrl: BASE_URL,
    counts: { pass: passCount, fail: failCount, total: checks.length },
    checks,
};

await fs.writeFile(path.join(ARTIFACTS_DIR, "summary.json"), JSON.stringify(summary, null, 2) + "\n");
await fs.writeFile(path.join(ARTIFACTS_DIR, "pages.json"), JSON.stringify(pages, null, 2) + "\n");
await fs.writeFile(path.join(ARTIFACTS_DIR, "console.json"), JSON.stringify(consoleAll, null, 2) + "\n");
await fs.writeFile(path.join(ARTIFACTS_DIR, "timeline.json"), JSON.stringify(timeline, null, 2) + "\n");

console.log(JSON.stringify({ verdict, counts: summary.counts }, null, 2));
process.exit(verdict === "PASS" ? 0 : 1);
JS

# Detect an already-installed Chromium binary. Playwright prefers to download
# its own browser build, but on hosts where the installed Chromium revision does
# not match the Playwright package version we fall back to whatever binary is
# present — Playwright's CDP wrapper is version-tolerant within a wide window.
pick_chromium() {
    if [[ -n "${AMS_CHROMIUM_EXECUTABLE:-}" && -x "${AMS_CHROMIUM_EXECUTABLE}" ]]; then
        printf '%s' "$AMS_CHROMIUM_EXECUTABLE"; return 0
    fi
    local candidates=(
        "$HOME/.cache/ms-playwright/chromium-"*/chrome-linux64/chrome
        "$HOME/.cache/ms-playwright/chromium-"*/chrome-linux/chrome
        "$HOME/.cache/ms-playwright/chromium_headless_shell-"*/chrome-headless-shell-linux64/chrome-headless-shell
        /usr/bin/google-chrome
        /usr/bin/chromium
        /usr/bin/chromium-browser
    )
    local c
    for c in "${candidates[@]}"; do
        if [[ -x "$c" ]]; then
            printf '%s' "$c"; return 0
        fi
    done
    return 1
}

CHROMIUM_PATH="$(pick_chromium || true)"
if [[ -n "$CHROMIUM_PATH" ]]; then
    log "Chromium executable: $CHROMIUM_PATH"
else
    log "No local Chromium found; Playwright will attempt to use its bundled binary."
fi

AMS_VERIFY_BASE_URL="$BASE_URL" \
AMS_VERIFY_ARTIFACTS_DIR="$ARTIFACTS_DIR" \
AMS_CHROMIUM_EXECUTABLE="${CHROMIUM_PATH:-}" \
    node "$DRIVER_JS" \
    >"$DRIVER_OUT" 2>"$DRIVER_ERR" &
DRIVER_PID=$!

# Forward driver output while it runs so operators see live progress without
# losing the captured stdout/stderr files used as evidence.
tail --pid="$DRIVER_PID" -n +1 -f "$DRIVER_OUT" 2>/dev/null &
TAIL_PID=$!

set +e
wait "$DRIVER_PID"
DRIVER_STATUS=$?
set -e

# Clean up the tail follower; it will naturally exit once --pid is gone, but
# be explicit so we do not leak it on script cleanup.
kill "$TAIL_PID" 2>/dev/null || true

if [[ $DRIVER_STATUS -ne 0 ]]; then
    log "Driver reported failures (exit=$DRIVER_STATUS). See $ARTIFACTS_DIR/summary.json."
    tail -n 40 "$DRIVER_ERR" >&2 || true
    exit "$DRIVER_STATUS"
fi

log "All verification checks passed."
exit 0
