#!/usr/bin/env bash
# M007/S02/T06 Prep UI verification harness.
#
# Starts the Ams.Workstation.Server locally, drives Chromium via Playwright to
# exercise the /prep migration proof (route + a11y + theme + Bit-free visuals)
# on migrated surfaces, and emits a pages/console/timeline/summary evidence
# bundle under .artifacts/browser/<timestamp>-m007-s02-uat/. Exits non-zero if
# any assertion fails so CI/automation can trust the verdict.
#
# Optional environment:
#   PRECONDITION_WORKING_DIR  Absolute path to a pre-seeded workstation
#                             workspace directory. If set, the driver types it
#                             into the header's working-directory input and
#                             clicks Set before running seeded assertion blocks
#                             (build-index-form when book-index.json is absent,
#                             or queue-builder/throughput/tabs/dialog when it
#                             is present). If unset, the harness still runs the
#                             empty-workspace + route + theme + a11y blocks.
#   AMS_VERIFY_PORT           Override the ephemeral port the server binds.
#   AMS_VERIFY_HOST           Loopback host (default 127.0.0.1).
#   AMS_VERIFY_STARTUP_TIMEOUT Seconds to wait for the server (default 90).
#   AMS_PLAYWRIGHT_VERSION    npm playwright version (default 1.59.1).
#   AMS_CHROMIUM_EXECUTABLE   Explicit Chromium binary path.
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
ARTIFACTS_DIR="$REPO_ROOT/.artifacts/browser/${TS}-m007-s02-uat"
mkdir -p "$ARTIFACTS_DIR"
mkdir -p "$PW_CACHE_DIR"

SERVER_LOG="$ARTIFACTS_DIR/server.log"
DRIVER_OUT="$ARTIFACTS_DIR/driver.stdout.log"
DRIVER_ERR="$ARTIFACTS_DIR/driver.stderr.log"
DRIVER_JS="$PW_CACHE_DIR/driver-s02.mjs"
DRIVER_POINTER="$ARTIFACTS_DIR/driver.mjs.path"

SERVER_PID=""

# BlazorWorkspace auto-loads `workingDirectory` from this path on startup, which
# would pollute the "no-workspace" assertion block on the next unseeded run.
# The harness backs it up and restores it on exit so developer state survives.
WORKSTATION_STATE_FILE="${AMS_WORKSTATION_STATE_FILE:-$HOME/.local/share/AMS/workstation-state.json}"
WORKSTATION_STATE_BACKUP=""
if [[ -f "$WORKSTATION_STATE_FILE" ]]; then
    WORKSTATION_STATE_BACKUP="${WORKSTATION_STATE_FILE}.verify-m007-s02.bak.$$"
    mv "$WORKSTATION_STATE_FILE" "$WORKSTATION_STATE_BACKUP"
fi

log() {
    printf '[verify_m007_s02_prep_ui] %s\n' "$*" >&2
}

cleanup() {
    local exit_code=$?
    if [[ -n "$SERVER_PID" ]] && kill -0 "$SERVER_PID" 2>/dev/null; then
        log "Stopping workstation server (pid=$SERVER_PID)"
        kill "$SERVER_PID" 2>/dev/null || true
        for _ in 1 2 3 4 5; do
            if ! kill -0 "$SERVER_PID" 2>/dev/null; then break; fi
            sleep 0.3
        done
        if kill -0 "$SERVER_PID" 2>/dev/null; then
            kill -9 "$SERVER_PID" 2>/dev/null || true
        fi
    fi
    # Restore the developer's workstation-state.json if we moved it aside.
    if [[ -n "$WORKSTATION_STATE_BACKUP" && -f "$WORKSTATION_STATE_BACKUP" ]]; then
        # Harness writes its own state-file during the seeded run; overwrite
        # only if the backup is still present so we don't stomp fresh data.
        mv -f "$WORKSTATION_STATE_BACKUP" "$WORKSTATION_STATE_FILE" 2>/dev/null || true
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
if [[ -n "${PRECONDITION_WORKING_DIR:-}" ]]; then
    log "Precondition working dir: $PRECONDITION_WORKING_DIR"
else
    log "PRECONDITION_WORKING_DIR unset — seeded-workspace assertions will be skipped."
fi

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
        printf '{"name":"ams-s02-pw-driver","private":true,"version":"0.0.0","type":"module"}\n' >package.json && \
        npm install --silent --no-audit --no-fund "playwright@${PLAYWRIGHT_VERSION}" >"$ARTIFACTS_DIR/pw-install.log" 2>&1 )
fi

printf '%s\n' "$DRIVER_JS" >"$DRIVER_POINTER"

cat >"$DRIVER_JS" <<'JS'
// Playwright UAT driver for M007/S02/T06.
//
// Exercises the migrated /prep surface at 360/768/1280 px:
//   route load + no JS pageerror, data-ams-theme present, no horizontal
//   overflow, >=44px touch targets, empty-workspace message, optional seeded
//   workspace flow (build-index-form OR queue-builder/throughput/tabs +
//   Pipeline Settings modal a11y), theme-toggle inheritance.
// Emits pages.json, console.json, timeline.json, summary.json.
import { chromium } from "playwright";
import fs from "node:fs/promises";
import path from "node:path";

const BASE_URL = process.env.AMS_VERIFY_BASE_URL;
const ARTIFACTS_DIR = process.env.AMS_VERIFY_ARTIFACTS_DIR;
const PRECONDITION_DIR = process.env.PRECONDITION_WORKING_DIR || "";
if (!BASE_URL || !ARTIFACTS_DIR) {
    console.error("Missing AMS_VERIFY_BASE_URL or AMS_VERIFY_ARTIFACTS_DIR");
    process.exit(2);
}

const VIEWPORTS = [
    { name: "mobile-360", width: 360, height: 780 },
    { name: "tablet-768", width: 768, height: 1024 },
    { name: "desktop-1280", width: 1280, height: 900 },
];

/** @type {Array<{name:string, viewport?:string, status:"PASS"|"FAIL", detail?:string, data?:any}>} */
const checks = [];
/** @type {Array<{t:number, kind:string, detail:string}>} */
const timeline = [];
/** @type {Array<any>} */
const pages = [];
/** @type {Array<any>} */
const consoleAll = [];

const t0 = Date.now();
function mark(kind, detail) {
    timeline.push({ t: Date.now() - t0, kind, detail });
}
function record(name, status, detail, data, viewport) {
    checks.push({ name, viewport, status, detail, data });
    mark("check", `${status} ${name}${viewport ? " @" + viewport : ""}${detail ? " :: " + detail : ""}`);
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
        const stack = err.stack || err.message || String(err);
        errors.push(stack);
        // Also stamp into the chronological console log as a pageerror so the
        // summary can assert on zero pageerrors across all viewports.
        consoleAll.push({ type: "pageerror", text: stack, url: page.url(), context, viewport: viewport?.name });
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

// Sleep helper — Blazor Server attaches @onclick handlers only after the
// SignalR circuit is live, and we cannot poll for Blazor private globals in
// release builds. A short settle lets the circuit come up.
async function settleForCircuit(page, ms = 1500) {
    await page.waitForTimeout(ms);
}

async function seedWorkspaceIfRequested(page) {
    if (!PRECONDITION_DIR) return { seeded: false };
    mark("seed", `typing working dir: ${PRECONDITION_DIR}`);
    await page.waitForSelector('[data-ams-header-control="working-directory"]', { state: "attached", timeout: 15000 });
    await settleForCircuit(page);

    // Clear and type the precondition path, then click Set.
    const input = await page.$('[data-ams-header-control="working-directory"]');
    if (!input) {
        record("seed.input_present", "FAIL", "working-directory input not found");
        return { seeded: false };
    }
    await input.click({ clickCount: 3 }).catch(() => { });
    await input.fill(PRECONDITION_DIR);
    // Nudge Blazor's @oninput binding — fill() issues one input event, which
    // matches @oninput semantics; but the Set button binds on the current
    // state, so we don't need an Enter key.
    await page.waitForTimeout(250);

    // Locate the Set button — it is the primary sm button immediately after
    // the working-directory input. Use role+name for robustness.
    const setButton = page.getByRole("button", { name: /^Set$/ });
    try {
        await setButton.click({ timeout: 5000 });
    } catch (err) {
        record("seed.set_button_click", "FAIL", err?.message || String(err));
        return { seeded: false };
    }

    // Wait for workspace to take effect — SectionContent only renders the
    // inspector if HasWorkingDirectory && HasBookIndex. We can observe the
    // result by asking the page which prep state is active.
    await page.waitForTimeout(2000);
    const state = await page.evaluate(() => {
        const noWs = document.querySelector('[data-ams-prep="no-workspace-message"]');
        const buildIdx = document.querySelector('[data-ams-prep="build-index-form"]');
        const queueBuilder = document.querySelector('[data-ams-prep="queue-builder"]');
        const wdInput = document.querySelector('[data-ams-header-control="working-directory"]');
        return {
            hasNoWorkspace: !!noWs && noWs.offsetParent !== null,
            hasBuildIndexForm: !!buildIdx && buildIdx.offsetParent !== null,
            hasQueueBuilder: !!queueBuilder && queueBuilder.offsetParent !== null,
            workingDirValue: wdInput ? wdInput.value : null,
        };
    });
    mark("seed", `post-seed state=${JSON.stringify(state)}`);

    if (state.hasQueueBuilder) {
        return { seeded: true, mode: "book-index-present" };
    }
    if (state.hasBuildIndexForm) {
        return { seeded: true, mode: "book-index-missing" };
    }
    // Seed attempted but workspace did not initialize (invalid path?). Fall
    // back to empty-workspace assertions and surface a failure.
    record("seed.workspace_initialized", "FAIL",
        `seeded dir did not produce build-index-form or queue-builder; state=${JSON.stringify(state)}`);
    return { seeded: false };
}

async function runPerViewportBaseChecks(page, seedState) {
    for (const vp of VIEWPORTS) {
        mark("viewport", `/prep @ ${vp.name}`);
        await page.setViewportSize({ width: vp.width, height: vp.height });
        const resp = await page.goto(`${BASE_URL}/prep`, { waitUntil: "networkidle" });
        const status = resp?.status() ?? 0;
        if (!resp || status !== 200) {
            record(`prep.load`, "FAIL", `status=${status}`, null, vp.name);
            continue;
        }
        record(`prep.load`, "PASS", `status=${status}`, null, vp.name);
        await settleForCircuit(page);

        const measurements = await page.evaluate(() => {
            const docEl = document.documentElement;
            const prepPage = document.querySelector(".prep-page");
            const themeAttr = docEl.getAttribute("data-ams-theme");

            const amsButtons = Array.from(document.querySelectorAll("button.ams-btn")).map((el) => ({
                classes: el.getAttribute("class"),
                offsetHeight: el.offsetHeight,
                offsetWidth: el.offsetWidth,
                visible: el.offsetParent !== null,
                text: (el.textContent || "").trim().slice(0, 40),
                data: el.getAttribute("data-ams-prep"),
                // R020 touch-target check tracks only md/lg AmsButton sizes.
                // `sm` is intentionally compact for inline row actions / header
                // chrome (same scoping as scripts/verify_m007_s01_ui.sh).
                isMdOrLg: el.classList.contains("ams-btn--md") || el.classList.contains("ams-btn--lg"),
            }));

            return {
                themeAttr,
                prepPage: prepPage ? {
                    scrollWidth: prepPage.scrollWidth,
                    clientWidth: prepPage.clientWidth,
                    offsetHeight: prepPage.offsetHeight,
                } : null,
                amsButtons,
                noWorkspace: !!document.querySelector('[data-ams-prep="no-workspace-message"]'),
                buildIndexForm: !!document.querySelector('[data-ams-prep="build-index-form"]'),
                queueBuilder: !!document.querySelector('[data-ams-prep="queue-builder"]'),
            };
        });

        // data-ams-theme must be present.
        if (measurements.themeAttr) {
            record("prep.theme_attr", "PASS", `data-ams-theme=${measurements.themeAttr}`, null, vp.name);
        } else {
            record("prep.theme_attr", "FAIL", "html[data-ams-theme] missing", null, vp.name);
        }

        // No horizontal overflow in .prep-page.
        if (!measurements.prepPage) {
            record("prep.no_horizontal_overflow", "FAIL", ".prep-page not found", null, vp.name);
        } else {
            const overflow = measurements.prepPage.scrollWidth - measurements.prepPage.clientWidth;
            if (overflow <= 1) {
                record("prep.no_horizontal_overflow", "PASS",
                    `scrollWidth=${measurements.prepPage.scrollWidth} clientWidth=${measurements.prepPage.clientWidth}`,
                    measurements.prepPage, vp.name);
            } else {
                record("prep.no_horizontal_overflow", "FAIL",
                    `.prep-page overflow=${overflow} (sw=${measurements.prepPage.scrollWidth} cw=${measurements.prepPage.clientWidth})`,
                    measurements.prepPage, vp.name);
            }
        }

        // R020 touch targets (md/lg only — sm is by-design compact for row
        // actions and header chrome; see T03-SUMMARY and scripts/verify_m007_s01_ui.sh).
        // On /prep the migrated surface is almost entirely sm buttons, so when
        // zero md/lg buttons render we record a PASS with a documented reason.
        const visibleButtons = measurements.amsButtons.filter((b) => b.visible);
        const visibleMdLg = visibleButtons.filter((b) => b.isMdOrLg);
        if (visibleMdLg.length === 0) {
            record("prep.touch_target_any", "PASS",
                `no md/lg .ams-btn on /prep (${visibleButtons.length} sm buttons; R020 applies to md/lg only)`,
                null, vp.name);
            record("prep.touch_target_all", "PASS",
                `no md/lg .ams-btn to enforce 44px against (${visibleButtons.length} sm buttons present)`,
                null, vp.name);
        } else {
            const tall = visibleMdLg.filter((b) => b.offsetHeight >= 44);
            if (tall.length === 0) {
                record("prep.touch_target_any", "FAIL",
                    `no visible md/lg .ams-btn >=44px tall (max=${Math.max(...visibleMdLg.map((b) => b.offsetHeight))})`,
                    visibleMdLg, vp.name);
            } else {
                record("prep.touch_target_any", "PASS",
                    `${tall.length}/${visibleMdLg.length} visible md/lg .ams-btn >=44px tall`, null, vp.name);
            }
            const short = visibleMdLg.filter((b) => b.offsetHeight < 44);
            if (short.length === 0) {
                record("prep.touch_target_all", "PASS",
                    `all ${visibleMdLg.length} visible md/lg .ams-btn clear 44px`, null, vp.name);
            } else {
                record("prep.touch_target_all", "FAIL",
                    `${short.length} visible md/lg .ams-btn under 44px`, short, vp.name);
            }
        }

        // State matrix assertions.
        if (!seedState.seeded) {
            if (measurements.noWorkspace) {
                record("prep.state.empty_workspace_message", "PASS",
                    "[data-ams-prep='no-workspace-message'] present", null, vp.name);
            } else {
                record("prep.state.empty_workspace_message", "FAIL",
                    "no-workspace-message missing but PRECONDITION_WORKING_DIR unset", null, vp.name);
            }
        } else if (seedState.mode === "book-index-missing") {
            if (measurements.buildIndexForm) {
                record("prep.state.build_index_form_visible", "PASS",
                    "[data-ams-prep='build-index-form'] present", null, vp.name);
            } else {
                record("prep.state.build_index_form_visible", "FAIL",
                    "build-index-form missing for seeded no-index workspace", null, vp.name);
            }

            const formState = await page.evaluate(() => {
                const form = document.querySelector('[data-ams-prep="build-index-form"]');
                if (!form) return null;
                const input = form.querySelector("input.ams-input");
                const button = Array.from(form.querySelectorAll("button.ams-btn")).find(
                    (b) => (b.textContent || "").trim().includes("Build book index"),
                );
                return {
                    inputPresent: !!input,
                    inputHeight: input?.offsetHeight ?? 0,
                    buttonPresent: !!button,
                    buttonHeight: button?.offsetHeight ?? 0,
                    buttonLabel: button ? (button.textContent || "").trim() : null,
                };
            });
            if (formState && formState.inputPresent && formState.inputHeight >= 44
                && formState.buttonPresent && formState.buttonHeight >= 44) {
                record("prep.state.build_index_form_touch_targets", "PASS",
                    `input=${formState.inputHeight}px button=${formState.buttonHeight}px`, null, vp.name);
            } else {
                record("prep.state.build_index_form_touch_targets", "FAIL",
                    `form contents undersized: ${JSON.stringify(formState)}`, formState, vp.name);
            }
        } else if (seedState.mode === "book-index-present") {
            if (measurements.queueBuilder) {
                record("prep.state.queue_builder_visible", "PASS",
                    "[data-ams-prep='queue-builder'] present", null, vp.name);
            } else {
                record("prep.state.queue_builder_visible", "FAIL",
                    "queue-builder missing for seeded indexed workspace", null, vp.name);
            }
        }

        pages.push({
            url: page.url(),
            viewport: vp.name,
            context: "prep-viewport",
            measurements,
        });
    }
}

async function runSeededInteractionChecks(page, seedState) {
    if (!seedState.seeded || seedState.mode !== "book-index-present") {
        return;
    }
    await page.setViewportSize({ width: 1280, height: 900 });
    const resp = await page.goto(`${BASE_URL}/prep`, { waitUntil: "networkidle" });
    if (!resp || resp.status() !== 200) {
        record("prep.seeded.load", "FAIL", `status=${resp?.status()}`);
        return;
    }
    await settleForCircuit(page);

    // --- Queue builder: two range inputs, keyboard operability. ---
    const rangeState = await page.evaluate(() => {
        const qb = document.querySelector('[data-ams-prep="queue-builder"]');
        if (!qb) return { present: false };
        const inputs = Array.from(qb.querySelectorAll('input[type="range"]')).map((el) => ({
            min: el.min,
            max: el.max,
            step: el.step,
            value: el.value,
            ariaLabel: el.getAttribute("aria-label"),
            disabled: el.disabled,
        }));
        return { present: true, count: inputs.length, inputs };
    });
    if (!rangeState.present) {
        record("prep.queue_builder.present", "FAIL", "[data-ams-prep='queue-builder'] missing on desktop");
    } else if (rangeState.count !== 2) {
        record("prep.queue_builder.range_inputs", "FAIL",
            `expected 2 range inputs, found ${rangeState.count}`, rangeState);
    } else {
        record("prep.queue_builder.range_inputs", "PASS",
            `2 range inputs present; values=${rangeState.inputs.map((i) => i.value).join(",")}`);
    }

    // Keyboard operability: focus lower thumb, ArrowLeft then ArrowRight should
    // change value. Skip if disabled (batch running) — the range inputs bind
    // IsEnabled to !Session.IsRunning && !_isBatchRunning.
    if (rangeState.present && rangeState.count === 2 && !rangeState.inputs[0].disabled) {
        // Track the lower thumb value before/after ArrowRight to avoid the
        // boundary case where the thumb is already at the minimum and ArrowLeft
        // is a no-op.
        await page.focus('[data-ams-prep="queue-builder"] input[type="range"].ams-range-slider__input--lower');
        const before = await page.$eval(
            '[data-ams-prep="queue-builder"] input[type="range"].ams-range-slider__input--lower',
            (el) => el.value,
        );
        await page.keyboard.press("ArrowRight");
        await page.waitForTimeout(250);
        const after = await page.$eval(
            '[data-ams-prep="queue-builder"] input[type="range"].ams-range-slider__input--lower',
            (el) => el.value,
        );
        if (before !== after) {
            record("prep.queue_builder.lower_arrow_right", "PASS", `${before} -> ${after}`);
        } else {
            // Try ArrowLeft as a fallback — handles the case where the thumb
            // was already pinned at max.
            await page.keyboard.press("ArrowLeft");
            await page.waitForTimeout(250);
            const after2 = await page.$eval(
                '[data-ams-prep="queue-builder"] input[type="range"].ams-range-slider__input--lower',
                (el) => el.value,
            );
            if (before !== after2) {
                record("prep.queue_builder.lower_arrow_key", "PASS", `${before} -> ${after2} (fallback ArrowLeft)`);
            } else {
                record("prep.queue_builder.lower_arrow_key", "FAIL",
                    `ArrowRight/Left did not change lower value from ${before}`);
            }
        }
    } else if (rangeState.present && rangeState.count === 2 && rangeState.inputs[0].disabled) {
        record("prep.queue_builder.lower_arrow_key", "PASS",
            "range inputs disabled (session running) — keyboard check skipped");
    }

    // Add range button enabled / Run batch prep disabled (empty queue).
    const buttonState = await page.evaluate(() => {
        const addRange = Array.from(document.querySelectorAll("button.ams-btn")).find(
            (b) => (b.textContent || "").trim().includes("Add range"),
        );
        const runBatch = Array.from(document.querySelectorAll("button.ams-btn")).find(
            (b) => (b.textContent || "").trim().includes("Run batch prep"),
        );
        return {
            addRange: addRange ? { visible: addRange.offsetParent !== null, disabled: addRange.disabled, classes: addRange.className } : null,
            runBatch: runBatch ? { visible: runBatch.offsetParent !== null, disabled: runBatch.disabled, classes: runBatch.className } : null,
        };
    });
    if (buttonState.addRange?.visible && !buttonState.addRange.disabled) {
        record("prep.queue_builder.add_range_enabled", "PASS", "Add range visible + enabled");
    } else {
        record("prep.queue_builder.add_range_enabled", "FAIL",
            `Add range state=${JSON.stringify(buttonState.addRange)}`);
    }
    if (buttonState.runBatch?.visible && buttonState.runBatch.disabled) {
        record("prep.queue_builder.run_batch_disabled_until_queued", "PASS",
            "Run batch prep visible + disabled (empty queue)");
    } else {
        record("prep.queue_builder.run_batch_disabled_until_queued", "FAIL",
            `Run batch prep state=${JSON.stringify(buttonState.runBatch)}`);
    }

    // --- Throughput panel: 5 sub-elements with tokenized backgrounds. ---
    const throughputState = await page.evaluate(() => {
        const panel = document.querySelector('[data-ams-prep="throughput"]');
        if (!panel) return { present: false };
        const boxes = Array.from(panel.querySelectorAll(".pipeline-throughput-box"));
        const indicators = Array.from(panel.querySelectorAll(".pipeline-throughput-indicator")).map((el) => ({
            classes: el.getAttribute("class"),
            background: getComputedStyle(el).backgroundColor,
        }));
        return { present: true, boxCount: boxes.length, indicators };
    });
    if (!throughputState.present) {
        record("prep.throughput.present", "FAIL", "[data-ams-prep='throughput'] missing");
    } else if (throughputState.boxCount !== 5) {
        record("prep.throughput.five_boxes", "FAIL",
            `expected 5 throughput boxes, found ${throughputState.boxCount}`);
    } else {
        record("prep.throughput.five_boxes", "PASS", "5 throughput boxes present");
        const transparent = throughputState.indicators.filter(
            (i) => !i.background || i.background === "rgba(0, 0, 0, 0)" || i.background === "transparent",
        );
        if (throughputState.indicators.length === 5 && transparent.length === 0) {
            record("prep.throughput.tokenized_backgrounds", "PASS",
                "5 indicator elements, all with non-transparent background");
        } else {
            record("prep.throughput.tokenized_backgrounds", "FAIL",
                `indicators=${throughputState.indicators.length} transparent=${transparent.length}`,
                throughputState.indicators);
        }
    }

    // --- Tabs: pipeline-tabs wrapper with 2 role=tab buttons, switchable. ---
    const tabsInitial = await page.evaluate(() => {
        const wrapper = document.querySelector('[data-ams-prep="pipeline-tabs"]');
        if (!wrapper) return { present: false };
        const tabs = Array.from(wrapper.querySelectorAll('button[role="tab"]'));
        return {
            present: true,
            tabCount: tabs.length,
            tabs: tabs.map((el) => ({
                ariaSelected: el.getAttribute("aria-selected"),
                text: (el.textContent || "").trim(),
                offsetHeight: el.offsetHeight,
            })),
        };
    });
    if (!tabsInitial.present) {
        record("prep.tabs.present", "FAIL", "[data-ams-prep='pipeline-tabs'] missing");
    } else if (tabsInitial.tabCount !== 2) {
        record("prep.tabs.two_tabs", "FAIL",
            `expected 2 role=tab buttons, found ${tabsInitial.tabCount}`, tabsInitial.tabs);
    } else {
        record("prep.tabs.two_tabs", "PASS", `2 role=tab buttons: [${tabsInitial.tabs.map((t) => t.text).join(", ")}]`);

        // R020: every tab button >= 44px.
        const shortTabs = tabsInitial.tabs.filter((t) => t.offsetHeight < 44);
        if (shortTabs.length === 0) {
            record("prep.tabs.touch_target", "PASS", "all tab buttons >=44px tall");
        } else {
            record("prep.tabs.touch_target", "FAIL",
                `${shortTabs.length} tab button(s) under 44px`, shortTabs);
        }

        // Switch to the History tab (second tab) and verify selection flipped.
        try {
            await page.click('[data-ams-prep="pipeline-tabs"] button[role="tab"]:nth-of-type(2)');
            await page.waitForTimeout(500);
            const tabsAfter = await page.evaluate(() => {
                const wrapper = document.querySelector('[data-ams-prep="pipeline-tabs"]');
                const tabs = Array.from(wrapper.querySelectorAll('button[role="tab"]'));
                return tabs.map((el) => el.getAttribute("aria-selected"));
            });
            if (tabsAfter[0] === "false" && tabsAfter[1] === "true") {
                record("prep.tabs.aria_selected_flip", "PASS",
                    `after click: [${tabsAfter.join(", ")}]`);
            } else {
                record("prep.tabs.aria_selected_flip", "FAIL",
                    `after click: [${tabsAfter.join(", ")}] (expected [false, true])`);
            }

            // Reset to the Active Tasks tab so the dialog test runs under the
            // default tab state.
            await page.click('[data-ams-prep="pipeline-tabs"] button[role="tab"]:nth-of-type(1)');
            await page.waitForTimeout(300);
        } catch (err) {
            record("prep.tabs.aria_selected_flip", "FAIL", err?.message || String(err));
        }
    }

    // --- Pipeline Settings dialog a11y: Escape + backdrop close, ams-select >=6 options. ---
    try {
        // Open via the data-ams-prep button.
        await page.click('[data-ams-prep="pipeline-settings-button"]');
        await page.waitForSelector('[role="dialog"][aria-modal="true"]', { state: "visible", timeout: 5000 });
        record("prep.dialog.open_on_click", "PASS", "Pipeline Settings dialog appeared");

        // End-stage ams-select has >=6 options.
        const selectInfo = await page.evaluate(() => {
            const dlg = document.querySelector('[role="dialog"][aria-modal="true"]');
            if (!dlg) return null;
            const selects = Array.from(dlg.querySelectorAll("select.ams-select"));
            return selects.map((s) => ({
                optionCount: s.querySelectorAll("option").length,
                value: s.value,
            }));
        });
        if (selectInfo && selectInfo.some((s) => s.optionCount >= 6)) {
            record("prep.dialog.end_stage_options_ge_6", "PASS",
                `selects: ${JSON.stringify(selectInfo)}`);
        } else {
            record("prep.dialog.end_stage_options_ge_6", "FAIL",
                `no ams-select with >=6 options; selects=${JSON.stringify(selectInfo)}`);
        }

        // Escape → dialog closes. AmsDialog wires @onkeydown on the dialog
        // element (tabindex=-1); focus the dialog so the handler receives the
        // key event instead of document.body.
        await page.evaluate(() => {
            const dlg = document.querySelector('[role="dialog"][aria-modal="true"]');
            if (dlg && typeof dlg.focus === "function") dlg.focus();
        });
        await page.waitForTimeout(150);
        await page.keyboard.press("Escape");
        await page.waitForTimeout(500);
        const escapeClosed = await page.evaluate(
            () => !document.querySelector('[role="dialog"][aria-modal="true"]'),
        );
        if (escapeClosed) {
            record("prep.dialog.escape_closes", "PASS", "Escape dismissed dialog");
        } else {
            record("prep.dialog.escape_closes", "FAIL", "dialog still in DOM after Escape");
        }

        // Re-open and close via backdrop click.
        await page.click('[data-ams-prep="pipeline-settings-button"]');
        await page.waitForSelector('[role="dialog"][aria-modal="true"]', { state: "visible", timeout: 5000 });
        await page.click(".ams-dialog-backdrop");
        await page.waitForTimeout(500);
        const backdropClosed = await page.evaluate(
            () => !document.querySelector('[role="dialog"][aria-modal="true"]'),
        );
        if (backdropClosed) {
            record("prep.dialog.backdrop_click_closes", "PASS", "backdrop click dismissed dialog");
        } else {
            record("prep.dialog.backdrop_click_closes", "FAIL", "dialog still in DOM after backdrop click");
        }
    } catch (err) {
        record("prep.dialog.open_on_click", "FAIL", err?.message || String(err));
    }
}

async function runThemeInheritanceCheck(page) {
    mark("theme", "starting theme toggle inheritance check");
    await page.setViewportSize({ width: 1280, height: 900 });
    const resp = await page.goto(`${BASE_URL}/prep`, { waitUntil: "networkidle" });
    if (!resp || resp.status() !== 200) {
        record("prep.theme.navigate", "FAIL", `status=${resp?.status()}`);
        return;
    }

    await page.waitForSelector('[data-ams-header-control="theme-toggle"]', { state: "attached", timeout: 15000 });
    await settleForCircuit(page);

    const before = await page.evaluate(() => ({
        theme: document.documentElement.getAttribute("data-ams-theme"),
        bg: getComputedStyle(document.body).backgroundColor,
    }));
    await page.click('[data-ams-header-control="theme-toggle"]');
    await page.waitForFunction(
        (prev) => document.documentElement.getAttribute("data-ams-theme") !== prev,
        before.theme,
        { timeout: 5000 },
    ).catch(() => { });
    const afterFirst = await page.evaluate(() => ({
        theme: document.documentElement.getAttribute("data-ams-theme"),
        bg: getComputedStyle(document.body).backgroundColor,
    }));

    if (before.theme && afterFirst.theme && before.theme !== afterFirst.theme) {
        record("prep.theme.attribute_flip", "PASS",
            `${before.theme} -> ${afterFirst.theme}`);
    } else {
        record("prep.theme.attribute_flip", "FAIL",
            `before=${before.theme} after=${afterFirst.theme}`);
    }
    if (before.bg !== afterFirst.bg) {
        record("prep.theme.bg_changed", "PASS", `bg: ${before.bg} -> ${afterFirst.bg}`);
    } else {
        record("prep.theme.bg_changed", "FAIL",
            `body background did not change (${before.bg})`);
    }

    // Flip back so later runs observe the default.
    await page.click('[data-ams-header-control="theme-toggle"]');
    await page.waitForFunction(
        (prev) => document.documentElement.getAttribute("data-ams-theme") !== prev,
        afterFirst.theme,
        { timeout: 5000 },
    ).catch(() => { });

    pages.push({
        url: page.url(),
        context: "theme-toggle",
        before,
        afterFirst,
    });
}

async function assertNoPageErrors() {
    const pageerrors = consoleAll.filter((m) => m.type === "pageerror");
    if (pageerrors.length === 0) {
        record("prep.console.no_pageerror", "PASS", "no pageerror entries across all viewports");
    } else {
        record("prep.console.no_pageerror", "FAIL",
            `${pageerrors.length} pageerror entries`, pageerrors);
    }
}

async function main() {
    mark("driver", "launch chromium");
    const launchOpts = { args: ["--no-sandbox", "--disable-dev-shm-usage"] };
    if (process.env.AMS_CHROMIUM_EXECUTABLE) {
        launchOpts.executablePath = process.env.AMS_CHROMIUM_EXECUTABLE;
    }
    const browser = await chromium.launch(launchOpts);
    const context = await browser.newContext();
    const page = await context.newPage();

    // Seed first so the viewport matrix exercises the right state.
    const seedCap = await capturePage(page, "seed", null);
    let seedState = { seeded: false };
    try {
        // Seeding happens from /prep so the header is mounted and the page can
        // observe state transitions.
        await page.setViewportSize({ width: 1280, height: 900 });
        const resp = await page.goto(`${BASE_URL}/prep`, { waitUntil: "networkidle" });
        if (!resp || resp.status() !== 200) {
            record("prep.initial_load", "FAIL", `status=${resp?.status()}`);
        } else {
            record("prep.initial_load", "PASS", `status=${resp.status()}`);
            seedState = await seedWorkspaceIfRequested(page);
        }
    } finally {
        seedCap.teardown();
    }

    // Per-viewport base checks (route, theme attr, overflow, touch targets,
    // empty-workspace OR build-index-form OR queue-builder visibility).
    const baseCap = await capturePage(page, "prep-viewport", null);
    try {
        await runPerViewportBaseChecks(page, seedState);
    } finally {
        baseCap.teardown();
    }

    // Seeded-only deep checks.
    const seededCap = await capturePage(page, "prep-seeded", null);
    try {
        await runSeededInteractionChecks(page, seedState);
    } finally {
        seededCap.teardown();
    }

    // Theme toggle inheritance on /prep.
    const themeCap = await capturePage(page, "theme-toggle", null);
    try {
        await runThemeInheritanceCheck(page);
    } finally {
        themeCap.teardown();
    }

    // Aggregate pageerror check.
    await assertNoPageErrors();

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
    slice: "M007/S02",
    task: "T06",
    verdict,
    startedAt: runStartedAt,
    finishedAt: new Date().toISOString(),
    baseUrl: BASE_URL,
    preconditionWorkingDir: PRECONDITION_DIR || null,
    seeded: PRECONDITION_DIR ? "attempted" : "unseeded",
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
PRECONDITION_WORKING_DIR="${PRECONDITION_WORKING_DIR:-}" \
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

kill "$TAIL_PID" 2>/dev/null || true

if [[ $DRIVER_STATUS -ne 0 ]]; then
    log "Driver reported failures (exit=$DRIVER_STATUS). See $ARTIFACTS_DIR/summary.json."
    tail -n 40 "$DRIVER_ERR" >&2 || true
    exit "$DRIVER_STATUS"
fi

log "All verification checks passed."
exit 0
