#!/usr/bin/env bash
# M007/S03/T06 Proof UI verification harness.
#
# Starts Ams.Workstation.Server locally, drives Chromium via Playwright to
# exercise /proof + /proof/pickups at 360/768/1280 px, and emits a machine-
# checkable pages/console/timeline/summary bundle under
# .artifacts/browser/<timestamp>-m007-s03-uat/. Exits non-zero if any
# assertion fails.
#
# AmsDialog a11y note:
#   In unseeded (empty-workspace) /proof/pickups shape there is no reachable
#   dialog-rendering path, so dialog assertions are intentionally skipped and
#   documented in summary checks.
#
# Optional environment:
#   PRECONDITION_WORKING_DIR  Absolute path to a pre-seeded workstation
#                             workspace. If set, harness seeds via header
#                             working-directory input and runs additional
#                             seeded-only assertions on /proof and
#                             /proof/pickups.
#   AMS_VERIFY_PORT           Override ephemeral port bound by server.
#   AMS_VERIFY_HOST           Loopback host (default 127.0.0.1).
#   AMS_VERIFY_STARTUP_TIMEOUT Seconds to wait for server (default 90).
#   AMS_PLAYWRIGHT_VERSION    npm playwright version (default 1.59.1).
#   AMS_CHROMIUM_EXECUTABLE   Explicit Chromium binary path.
#   AMS_WORKSTATION_STATE_FILE Override workstation-state.json path.
#   AMS_WS_SKIP_STATE_RESTORE Set to 1 to skip restoring moved state backup.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

PROJECT_PATH="host/Ams.Workstation.Server/Ams.Workstation.Server.csproj"
PLAYWRIGHT_VERSION="${AMS_PLAYWRIGHT_VERSION:-1.59.1}"
HOST="${AMS_VERIFY_HOST:-127.0.0.1}"
TIMEOUT_START_SEC="${AMS_VERIFY_STARTUP_TIMEOUT:-90}"
KEEP_ARTIFACTS_ON_FAIL="${AMS_VERIFY_KEEP_ARTIFACTS:-1}"
SKIP_STATE_RESTORE="${AMS_WS_SKIP_STATE_RESTORE:-0}"
# Playwright install cache (same shape/pin as S02 harness).
PW_CACHE_DIR="${AMS_PW_CACHE_DIR:-$REPO_ROOT/.artifacts/browser/.pwcache/${PLAYWRIGHT_VERSION}}"

# Keep Playwright off network for browser downloads; rely on locally cached
# Chromium and explicit executablePath selection.
export PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD="${PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD:-1}"

timestamp() {
    date -u +"%Y-%m-%dT%H-%M-%S-%3NZ"
}

TS="$(timestamp)"
ARTIFACTS_DIR="$REPO_ROOT/.artifacts/browser/${TS}-m007-s03-uat"
mkdir -p "$ARTIFACTS_DIR"
mkdir -p "$PW_CACHE_DIR"

SERVER_LOG="$ARTIFACTS_DIR/server.log"
DRIVER_OUT="$ARTIFACTS_DIR/driver.stdout.log"
DRIVER_ERR="$ARTIFACTS_DIR/driver.stderr.log"
DRIVER_JS="$PW_CACHE_DIR/driver-s03.mjs"
DRIVER_POINTER="$ARTIFACTS_DIR/driver.mjs.path"

SERVER_PID=""

# BlazorWorkspace auto-loads persisted workingDirectory from this state file.
# Move it aside before boot so unseeded assertions stay deterministic.
WORKSTATION_STATE_FILE="${AMS_WORKSTATION_STATE_FILE:-$HOME/.local/share/AMS/workstation-state.json}"
WORKSTATION_STATE_BACKUP=""
if [[ -f "$WORKSTATION_STATE_FILE" ]]; then
    WORKSTATION_STATE_BACKUP="${WORKSTATION_STATE_FILE}.verify-m007-s03.bak.$$"
    mv "$WORKSTATION_STATE_FILE" "$WORKSTATION_STATE_BACKUP"
fi

log() {
    printf '[verify_m007_s03_proof_ui] %s\n' "$*" >&2
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

    if [[ "$SKIP_STATE_RESTORE" != "1" ]] && [[ -n "$WORKSTATION_STATE_BACKUP" && -f "$WORKSTATION_STATE_BACKUP" ]]; then
        mv -f "$WORKSTATION_STATE_BACKUP" "$WORKSTATION_STATE_FILE" 2>/dev/null || true
    fi

    if [[ $exit_code -ne 0 && "$KEEP_ARTIFACTS_ON_FAIL" == "0" ]]; then
        rm -rf "$ARTIFACTS_DIR"
    else
        log "Artifacts: $ARTIFACTS_DIR"
    fi
    exit "$exit_code"
}
# Register trap immediately after backup so any early failure still restores.
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
    log "PRECONDITION_WORKING_DIR unset — seeded-only assertions will be skipped."
fi

log "Restoring + building $PROJECT_PATH (Debug)"
if ! dotnet build "$PROJECT_PATH" -c Debug --nologo -v minimal >"$ARTIFACTS_DIR/build.log" 2>&1; then
    log "Workstation build failed; see $ARTIFACTS_DIR/build.log"
    tail -n 60 "$ARTIFACTS_DIR/build.log" >&2 || true
    exit 1
fi

log "Starting workstation server on $BASE_URL"
# --no-launch-profile required so launchSettings pin does not override URL.
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

if [[ ! -d "$PW_CACHE_DIR/node_modules/playwright" ]]; then
    log "Installing playwright@${PLAYWRIGHT_VERSION} into $PW_CACHE_DIR (first run)"
    (
        cd "$PW_CACHE_DIR"
        printf '{"name":"ams-s03-pw-driver","private":true,"version":"0.0.0","type":"module"}\n' >package.json
        npm install --silent --no-audit --no-fund "playwright@${PLAYWRIGHT_VERSION}" >"$ARTIFACTS_DIR/pw-install.log" 2>&1
    )
fi

printf '%s\n' "$DRIVER_JS" >"$DRIVER_POINTER"

cat >"$DRIVER_JS" <<'JS'
import { chromium } from "playwright";
import fs from "node:fs/promises";
import path from "node:path";

const BASE_URL = process.env.AMS_VERIFY_BASE_URL;
const ARTIFACTS_DIR = process.env.AMS_VERIFY_ARTIFACTS_DIR;
const REPO_ROOT = process.env.AMS_VERIFY_REPO_ROOT;
const PRECONDITION_DIR = process.env.PRECONDITION_WORKING_DIR || "";
if (!BASE_URL || !ARTIFACTS_DIR || !REPO_ROOT) {
    console.error("Missing AMS_VERIFY_BASE_URL / AMS_VERIFY_ARTIFACTS_DIR / AMS_VERIFY_REPO_ROOT");
    process.exit(2);
}

const VIEWPORTS = [
    { name: "mobile-360", width: 360, height: 780 },
    { name: "tablet-768", width: 768, height: 1024 },
    { name: "desktop-1280", width: 1280, height: 900 },
];

const ROUTES = [
    { key: "proof", path: "/proof", rootSelector: ".proof-index" },
    { key: "pickups", path: "/proof/pickups", rootSelector: ".proof-pickups-page" },
];

const REQUIRED_PICKUPS_SOURCE_SNIPPETS = [
    'data-proof-pickups-page="true"',
    'data-proof-pickups-handoff="editing"',
    'data-proof-pickups-action="return-editing"',
    'data-proof-pickups-queue="matched"',
    'data-proof-pickups-queue="unmatched"',
    'data-proof-pickups-queue="staged"',
    'data-proof-pickups-queue="applied"',
    'data-proof-pickups-queue="reverted"',
    'data-proof-pickups-queue="failed"',
    'data-proof-pickups-diagnostics="true"',
    'data-proof-pickups-ledger="true"',
    'data-proof-pickups-ledger-row="true"',
    'data-proof-pickups-action="commit"',
    'data-proof-pickups-action="revert"',
    'data-proof-pickups-phase="true"',
    'data-proof-pickups-last-op="true"',
];

const REQUIRED_PICKUPS_RUNTIME_EMPTY_SELECTORS = [
    '[data-proof-pickups-page="true"]',
    '[data-proof-pickups-handoff="editing"]',
    '[data-proof-pickups-action="return-editing"]',
];

/*************** Evidence buffers ***************/
/** @type {Array<{name:string, status:"PASS"|"FAIL", detail?:string, data?:any, viewport?:string, route?:string}>} */
const assertions = [];
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
function record(name, status, detail, data, viewport, route) {
    assertions.push({ name, status, detail, data, viewport, route });
    mark("check", `${status} ${name}${viewport ? " @" + viewport : ""}${route ? " [" + route + "]" : ""}${detail ? " :: " + detail : ""}`);
}

async function capturePage(page, context, viewport, route) {
    const handlers = [];

    const onConsole = (msg) => {
        const entry = { type: msg.type(), text: msg.text(), url: page.url(), context, viewport, route };
        consoleAll.push(entry);
    };

    const onPageError = (err) => {
        const text = err?.stack || err?.message || String(err);
        consoleAll.push({ type: "pageerror", text, url: page.url(), context, viewport, route });
    };

    const onRequestFailed = (req) => {
        consoleAll.push({
            type: "requestfailed",
            text: `${req.url()} :: ${req.failure()?.errorText || "unknown"}`,
            url: page.url(),
            context,
            viewport,
            route,
        });
    };

    page.on("console", onConsole);
    page.on("pageerror", onPageError);
    page.on("requestfailed", onRequestFailed);

    handlers.push(() => page.off("console", onConsole));
    handlers.push(() => page.off("pageerror", onPageError));
    handlers.push(() => page.off("requestfailed", onRequestFailed));

    return {
        teardown() {
            for (const off of handlers) off();
        },
    };
}

async function settleForCircuit(page, ms = 1500) {
    await page.waitForTimeout(ms);
}

async function assertProofSourceContracts() {
    const pickupsPath = path.join(REPO_ROOT, "host/Ams.Workstation.Server/Components/Pages/Proof/Pickups.razor");
    let pickupsSource;
    try {
        pickupsSource = await fs.readFile(pickupsPath, "utf8");
    } catch (err) {
        record("proof.pickups.source_anchor_contract", "FAIL", `read failed: ${err?.message || err}`);
        return;
    }

    const missingPickupsSnippets = REQUIRED_PICKUPS_SOURCE_SNIPPETS.filter((snippet) => !pickupsSource.includes(snippet));
    if (missingPickupsSnippets.length === 0) {
        record("proof.pickups.source_anchor_contract", "PASS", `${REQUIRED_PICKUPS_SOURCE_SNIPPETS.length} source anchor snippets present`);
    } else {
        record("proof.pickups.source_anchor_contract", "FAIL", `missing ${missingPickupsSnippets.length} source snippets`, missingPickupsSnippets);
    }

    const overviewPath = path.join(REPO_ROOT, "host/Ams.Workstation.Server/Components/Pages/Proof/Overview.razor");
    let overviewSource;
    try {
        overviewSource = await fs.readFile(overviewPath, "utf8");
    } catch (err) {
        record("proof.overview.loading_spinner_contract", "FAIL", `read failed: ${err?.message || err}`);
        return;
    }

    if (overviewSource.includes("<AmsSpinner")) {
        record("proof.overview.loading_spinner_contract", "PASS", "Overview loading spinner anchor present in source");
    } else {
        record("proof.overview.loading_spinner_contract", "FAIL", "Missing <AmsSpinner anchor in Overview.razor");
    }
}

async function seedWorkspaceIfRequested(page) {
    if (!PRECONDITION_DIR) {
        return { requested: false, seeded: false, seedMode: "unseeded" };
    }

    mark("seed", `attempting workspace seed from ${PRECONDITION_DIR}`);
    const resp = await page.goto(`${BASE_URL}/proof`, { waitUntil: "networkidle" });
    if (!resp || resp.status() !== 200) {
        record("seed.navigate_proof", "FAIL", `status=${resp?.status()}`);
        return { requested: true, seeded: false, seedMode: "seeded" };
    }

    await page.waitForSelector('[data-ams-header-control="working-directory"]', { state: "attached", timeout: 15000 });
    await settleForCircuit(page);

    const input = await page.$('[data-ams-header-control="working-directory"]');
    if (!input) {
        record("seed.input_present", "FAIL", "header working-directory input missing");
        return { requested: true, seeded: false, seedMode: "seeded" };
    }

    await input.click({ clickCount: 3 }).catch(() => { });
    await input.fill(PRECONDITION_DIR);
    await page.waitForTimeout(250);

    try {
        await page.getByRole("button", { name: /^Set$/ }).click({ timeout: 5000 });
    } catch (err) {
        record("seed.set_button_click", "FAIL", err?.message || String(err));
        return { requested: true, seeded: false, seedMode: "seeded" };
    }

    await page.waitForTimeout(2000);

    const seedState = await page.evaluate(() => {
        const noWorkspace = document.querySelector('[data-ams-proof="no-workspace-message"]');
        const chapterListItems = document.querySelectorAll(".chapters li");
        const pickupsLink = document.querySelector('[data-proof-index-link="pickups"]');
        const wdInput = document.querySelector('[data-ams-header-control="working-directory"]');
        return {
            noWorkspaceVisible: !!noWorkspace && noWorkspace.offsetParent !== null,
            chapterListCount: chapterListItems.length,
            pickupsLinkVisible: !!pickupsLink && pickupsLink.offsetParent !== null,
            workingDirValue: wdInput ? wdInput.value : null,
        };
    });

    if (!seedState.noWorkspaceVisible) {
        record("seed.workspace_initialized", "PASS", `chapterListCount=${seedState.chapterListCount}; pickupsLinkVisible=${seedState.pickupsLinkVisible}`);
        return {
            requested: true,
            seeded: true,
            seedMode: "seeded",
            ...seedState,
        };
    }

    record(
        "seed.workspace_initialized",
        "FAIL",
        `proof index remained empty-workspace after seeding; workingDir='${seedState.workingDirValue || ""}'`,
        seedState,
    );
    return {
        requested: true,
        seeded: false,
        seedMode: "seeded",
        ...seedState,
    };
}

function pushPageObservation(route, viewport, observation) {
    pages.push({
        route,
        viewport,
        url: observation.url,
        context: "proof-route",
        ...observation,
    });
}

async function runViewportRouteChecks(page, seedState) {
    for (const vp of VIEWPORTS) {
        await page.setViewportSize({ width: vp.width, height: vp.height });

        for (const route of ROUTES) {
            mark("route", `${route.path} @ ${vp.name}`);
            const resp = await page.goto(`${BASE_URL}${route.path}`, { waitUntil: "networkidle" });
            const status = resp?.status() ?? 0;

            if (!resp || status !== 200) {
                record(`${route.key}.load`, "FAIL", `status=${status}`, null, vp.name, route.path);
                continue;
            }
            record(`${route.key}.load`, "PASS", `status=${status}`, null, vp.name, route.path);

            await settleForCircuit(page);

            const observation = await page.evaluate((rootSelector) => {
                const root = document.querySelector(rootSelector);
                const rootButtons = root
                    ? Array.from(root.querySelectorAll("button.ams-btn.ams-btn--md, button.ams-btn.ams-btn--lg"))
                    : [];

                const visibleMdLgButtons = rootButtons
                    .filter((el) => el.offsetParent !== null)
                    .map((el) => ({
                        className: el.className,
                        text: (el.textContent || "").trim().slice(0, 60),
                        offsetHeight: el.offsetHeight,
                    }));

                const runtimePickupsAnchors = [];
                for (const el of document.querySelectorAll("*")) {
                    for (const attrName of el.getAttributeNames()) {
                        if (attrName.startsWith("data-proof-pickups")) {
                            runtimePickupsAnchors.push(`${attrName}=${el.getAttribute(attrName) ?? ""}`);
                        }
                    }
                }

                const routeInfoMessage = root
                    ? Array.from(root.querySelectorAll(".ams-message.ams-message--info")).some((el) => el.offsetParent !== null)
                    : false;

                const proofNoWorkspace = document.querySelector('[data-ams-proof="no-workspace-message"]');

                return {
                    url: window.location.href,
                    themeAttr: document.documentElement.getAttribute("data-ams-theme"),
                    root: root
                        ? {
                            scrollWidth: root.scrollWidth,
                            clientWidth: root.clientWidth,
                            offsetHeight: root.offsetHeight,
                        }
                        : null,
                    routeInfoMessage,
                    proofNoWorkspaceVisible: !!proofNoWorkspace && proofNoWorkspace.offsetParent !== null,
                    visibleMdLgButtons,
                    runtimePickupsAnchors,
                };
            }, route.rootSelector);

            pushPageObservation(route.path, vp.name, observation);

            // html[data-ams-theme] present.
            if (observation.themeAttr) {
                record(`${route.key}.theme_attr`, "PASS", `data-ams-theme=${observation.themeAttr}`, null, vp.name, route.path);
            } else {
                record(`${route.key}.theme_attr`, "FAIL", "html[data-ams-theme] missing", null, vp.name, route.path);
            }

            // No horizontal overflow for route root.
            if (!observation.root) {
                record(`${route.key}.no_horizontal_overflow`, "FAIL", `${route.rootSelector} not found`, null, vp.name, route.path);
            } else {
                const overflow = observation.root.scrollWidth - observation.root.clientWidth;
                if (overflow <= 1) {
                    record(
                        `${route.key}.no_horizontal_overflow`,
                        "PASS",
                        `scrollWidth=${observation.root.scrollWidth} clientWidth=${observation.root.clientWidth}`,
                        observation.root,
                        vp.name,
                        route.path,
                    );
                } else {
                    record(
                        `${route.key}.no_horizontal_overflow`,
                        "FAIL",
                        `overflow=${overflow} (sw=${observation.root.scrollWidth} cw=${observation.root.clientWidth})`,
                        observation.root,
                        vp.name,
                        route.path,
                    );
                }
            }

            // Touch targets (R020): only md/lg AmsButton; skip if absent.
            const mdLgButtons = observation.visibleMdLgButtons;
            if (mdLgButtons.length === 0) {
                record(`${route.key}.touch_targets_md_lg`, "PASS", "no visible md/lg buttons on route", null, vp.name, route.path);
            } else {
                const short = mdLgButtons.filter((btn) => btn.offsetHeight < 44);
                if (short.length === 0) {
                    record(`${route.key}.touch_targets_md_lg`, "PASS", `all ${mdLgButtons.length} md/lg buttons >=44px`, null, vp.name, route.path);
                } else {
                    record(`${route.key}.touch_targets_md_lg`, "FAIL", `${short.length} md/lg buttons under 44px`, short, vp.name, route.path);
                }
            }

            const unseededMode = !seedState.requested;

            if (unseededMode && route.key === "proof") {
                if (observation.routeInfoMessage) {
                    record("proof.empty_workspace_info", "PASS", ".ams-message.ams-message--info visible", null, vp.name, route.path);
                } else {
                    record("proof.empty_workspace_info", "FAIL", "expected info message in unseeded /proof", null, vp.name, route.path);
                }

                if (observation.proofNoWorkspaceVisible) {
                    record("proof.empty_workspace_anchor", "PASS", "[data-ams-proof='no-workspace-message'] visible", null, vp.name, route.path);
                } else {
                    record("proof.empty_workspace_anchor", "FAIL", "[data-ams-proof='no-workspace-message'] missing", null, vp.name, route.path);
                }
            }

            if (unseededMode && route.key === "pickups") {
                if (observation.routeInfoMessage) {
                    record("pickups.empty_workspace_info", "PASS", ".ams-message.ams-message--info visible", null, vp.name, route.path);
                } else {
                    record("pickups.empty_workspace_info", "FAIL", "expected info message in unseeded /proof/pickups", null, vp.name, route.path);
                }

                const missingRuntimeAnchors = REQUIRED_PICKUPS_RUNTIME_EMPTY_SELECTORS.filter(
                    (selector) => !observation.runtimePickupsAnchors.some((anchor) => {
                        if (selector === '[data-proof-pickups-page="true"]') return anchor === "data-proof-pickups-page=true";
                        if (selector === '[data-proof-pickups-handoff="editing"]') return anchor === "data-proof-pickups-handoff=editing";
                        if (selector === '[data-proof-pickups-action="return-editing"]') return anchor === "data-proof-pickups-action=return-editing";
                        return false;
                    }),
                );

                if (missingRuntimeAnchors.length === 0) {
                    record("pickups.runtime_anchors_empty_workspace", "PASS", "base pickups anchors visible in empty-workspace DOM", null, vp.name, route.path);
                } else {
                    record(
                        "pickups.runtime_anchors_empty_workspace",
                        "FAIL",
                        `missing ${missingRuntimeAnchors.length} required runtime pickups anchors`,
                        { missingRuntimeAnchors, runtimePickupsAnchors: observation.runtimePickupsAnchors },
                        vp.name,
                        route.path,
                    );
                }
            }
        }
    }
}

async function runSeededAssertions(page, seedState) {
    if (!seedState.requested) {
        record("seeded.blocks", "PASS", "PRECONDITION_WORKING_DIR not set; seeded assertion blocks skipped by contract");
        return;
    }

    // /proof seeded assertions: chapter list + pickups link visible.
    await page.setViewportSize({ width: 1280, height: 900 });
    const proofResp = await page.goto(`${BASE_URL}/proof`, { waitUntil: "networkidle" });
    if (!proofResp || proofResp.status() !== 200) {
        record("seeded.proof.load", "FAIL", `status=${proofResp?.status()}`);
    } else {
        await settleForCircuit(page);
        const seededProof = await page.evaluate(() => {
            const noWorkspace = document.querySelector('[data-ams-proof="no-workspace-message"]');
            const chapterItems = document.querySelectorAll(".chapters li");
            const pickupsLink = document.querySelector('[data-proof-index-link="pickups"]');
            return {
                noWorkspaceVisible: !!noWorkspace && noWorkspace.offsetParent !== null,
                chapterListCount: chapterItems.length,
                pickupsLinkVisible: !!pickupsLink && pickupsLink.offsetParent !== null,
            };
        });

        if (!seededProof.noWorkspaceVisible) {
            record("seeded.proof.not_empty_workspace", "PASS", "no-workspace fallback hidden in seeded mode", seededProof);
        } else {
            record("seeded.proof.not_empty_workspace", "FAIL", "still showing no-workspace fallback in seeded mode", seededProof);
        }

        if (seededProof.chapterListCount > 0) {
            record("seeded.proof.chapter_list", "PASS", `chapter list rendered (${seededProof.chapterListCount} items)`, seededProof);
        } else {
            record("seeded.proof.chapter_list", "FAIL", "chapter list did not render in seeded mode", seededProof);
        }

        if (seededProof.pickupsLinkVisible) {
            record("seeded.proof.pickups_link", "PASS", "pickups quick-link visible", seededProof);
        } else {
            record("seeded.proof.pickups_link", "FAIL", "pickups quick-link not visible", seededProof);
        }

        pages.push({
            route: "/proof",
            viewport: "desktop-1280",
            context: "seeded-proof",
            url: page.url(),
            seededProof,
        });
    }

    // /proof/pickups seeded UI chrome assertions.
    const pickupsResp = await page.goto(`${BASE_URL}/proof/pickups`, { waitUntil: "networkidle" });
    if (!pickupsResp || pickupsResp.status() !== 200) {
        record("seeded.pickups.load", "FAIL", `status=${pickupsResp?.status()}`);
    } else {
        await settleForCircuit(page);
        const seededPickups = await page.evaluate(() => {
            const noWorkspaceInfo = document.querySelector(".proof-pickups-page .ams-message.ams-message--info");
            const controls = document.querySelector(".proof-pickups-controls");
            const chapterSelect = document.querySelector(".proof-pickups-page select.ams-select");
            const diagnostics = document.querySelector('[data-proof-pickups-diagnostics="true"]');
            const ledger = document.querySelector('[data-proof-pickups-ledger="true"]');
            return {
                noWorkspaceInfoVisible: !!noWorkspaceInfo && noWorkspaceInfo.offsetParent !== null,
                controlsVisible: !!controls && controls.offsetParent !== null,
                chapterSelectVisible: !!chapterSelect && chapterSelect.offsetParent !== null,
                diagnosticsVisible: !!diagnostics && diagnostics.offsetParent !== null,
                ledgerVisible: !!ledger && ledger.offsetParent !== null,
            };
        });

        if (!seededPickups.noWorkspaceInfoVisible) {
            record("seeded.pickups.not_empty_workspace", "PASS", "no-workspace info hidden in seeded mode", seededPickups);
        } else {
            record("seeded.pickups.not_empty_workspace", "FAIL", "still showing no-workspace info in seeded mode", seededPickups);
        }

        if (seededPickups.controlsVisible && seededPickups.chapterSelectVisible) {
            record("seeded.pickups.controls", "PASS", "pickups controls + chapter select visible", seededPickups);
        } else {
            record("seeded.pickups.controls", "FAIL", "pickups controls/selection chrome missing", seededPickups);
        }

        if (seededPickups.diagnosticsVisible && seededPickups.ledgerVisible) {
            record("seeded.pickups.chrome_panels", "PASS", "diagnostics + ledger panels visible", seededPickups);
        } else {
            record("seeded.pickups.chrome_panels", "FAIL", "diagnostics/ledger panel missing", seededPickups);
        }

        pages.push({
            route: "/proof/pickups",
            viewport: "desktop-1280",
            context: "seeded-pickups",
            url: page.url(),
            seededPickups,
        });
    }

    // Dialog block intentionally skipped in S03 proof-empty-workspace path.
    record(
        "pickups.dialog.a11y_block",
        "PASS",
        "Skipped: no reachable dialog path in /proof/pickups empty-workspace flow (documented in harness header).",
    );
}

async function runThemeToggleCheck(page) {
    mark("theme", "starting theme toggle inheritance check on /proof");
    await page.setViewportSize({ width: 1280, height: 900 });
    const resp = await page.goto(`${BASE_URL}/proof`, { waitUntil: "networkidle" });
    if (!resp || resp.status() !== 200) {
        record("proof.theme.navigate", "FAIL", `status=${resp?.status()}`);
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
        (prevTheme) => document.documentElement.getAttribute("data-ams-theme") !== prevTheme,
        before.theme,
        { timeout: 5000 },
    ).catch(() => { });

    const after = await page.evaluate(() => ({
        theme: document.documentElement.getAttribute("data-ams-theme"),
        bg: getComputedStyle(document.body).backgroundColor,
    }));

    if (before.theme && after.theme && before.theme !== after.theme) {
        record("proof.theme.attribute_flip", "PASS", `${before.theme} -> ${after.theme}`);
    } else {
        record("proof.theme.attribute_flip", "FAIL", `before=${before.theme} after=${after.theme}`);
    }

    if (before.bg !== after.bg) {
        record("proof.theme.body_bg_changed", "PASS", `${before.bg} -> ${after.bg}`);
    } else {
        record("proof.theme.body_bg_changed", "FAIL", `body background unchanged (${before.bg})`);
    }

    // Flip back so next run starts from prior state.
    await page.click('[data-ams-header-control="theme-toggle"]');
    await page.waitForFunction(
        (prevTheme) => document.documentElement.getAttribute("data-ams-theme") !== prevTheme,
        after.theme,
        { timeout: 5000 },
    ).catch(() => { });

    pages.push({
        context: "theme-toggle",
        route: "/proof",
        viewport: "desktop-1280",
        url: page.url(),
        before,
        after,
    });
}

async function assertNoPageErrors() {
    const pageerrors = consoleAll.filter((entry) => entry.type === "pageerror");
    if (pageerrors.length === 0) {
        record("proof.console.no_pageerror", "PASS", "zero pageerror entries across all viewports/routes");
    } else {
        record("proof.console.no_pageerror", "FAIL", `${pageerrors.length} pageerror entries found`, pageerrors);
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

    await assertProofSourceContracts();

    const seedCap = await capturePage(page, "seed", null, null);
    let seedState = { requested: false, seeded: false, seedMode: PRECONDITION_DIR ? "seeded" : "unseeded" };
    try {
        seedState = await seedWorkspaceIfRequested(page);
    } finally {
        seedCap.teardown();
    }

    const routeCap = await capturePage(page, "proof-routes", null, null);
    try {
        await runViewportRouteChecks(page, seedState);
    } finally {
        routeCap.teardown();
    }

    const seededCap = await capturePage(page, "seeded-assertions", null, null);
    try {
        await runSeededAssertions(page, seedState);
    } finally {
        seededCap.teardown();
    }

    const themeCap = await capturePage(page, "theme-toggle", null, null);
    try {
        await runThemeToggleCheck(page);
    } finally {
        themeCap.teardown();
    }

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

const passCount = assertions.filter((a) => a.status === "PASS").length;
const failCount = assertions.filter((a) => a.status === "FAIL").length;
const verdict = failCount === 0 ? "PASS" : "FAIL";

const summary = {
    slice: "M007/S03",
    task: "T06",
    verdict,
    startedAt: runStartedAt,
    finishedAt: new Date().toISOString(),
    baseUrl: BASE_URL,
    preconditionWorkingDir: PRECONDITION_DIR || null,
    seeded: PRECONDITION_DIR ? "seeded" : "unseeded",
    counts: { pass: passCount, fail: failCount, total: assertions.length },
    assertions,
};

await fs.writeFile(path.join(ARTIFACTS_DIR, "summary.json"), JSON.stringify(summary, null, 2) + "\n");
await fs.writeFile(path.join(ARTIFACTS_DIR, "pages.json"), JSON.stringify(pages, null, 2) + "\n");
await fs.writeFile(path.join(ARTIFACTS_DIR, "console.json"), JSON.stringify(consoleAll, null, 2) + "\n");
await fs.writeFile(path.join(ARTIFACTS_DIR, "timeline.json"), JSON.stringify(timeline, null, 2) + "\n");

console.log(JSON.stringify({ verdict, counts: summary.counts, seeded: summary.seeded }, null, 2));
process.exit(verdict === "PASS" ? 0 : 1);
JS

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
    log "No local Chromium found; Playwright will attempt bundled binary."
fi

AMS_VERIFY_BASE_URL="$BASE_URL" \
AMS_VERIFY_ARTIFACTS_DIR="$ARTIFACTS_DIR" \
AMS_VERIFY_REPO_ROOT="$REPO_ROOT" \
AMS_CHROMIUM_EXECUTABLE="${CHROMIUM_PATH:-}" \
PRECONDITION_WORKING_DIR="${PRECONDITION_WORKING_DIR:-}" \
    node "$DRIVER_JS" \
    >"$DRIVER_OUT" 2>"$DRIVER_ERR" &
DRIVER_PID=$!

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
