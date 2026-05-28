const reconnectModal = document.getElementById("components-reconnect-modal");
const retryButton    = document.getElementById("components-reconnect-button");
const resumeButton   = document.getElementById("components-resume-button");
const secondsSpan    = document.getElementById("components-seconds-to-next-attempt");

if (!reconnectModal || !retryButton || !resumeButton) {
    console.error("[ReconnectModal] Required DOM elements are missing. Reconnect UI will not function.");
} else {
    reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);
    retryButton.addEventListener("click", retry);
    resumeButton.addEventListener("click", resume);
}

let retryOnVisibility = false;

function handleReconnectStateChanged(event) {
    const state = event.detail.state;

    if (secondsSpan && event.detail.secondsToNextAttempt != null) {
        secondsSpan.textContent = event.detail.secondsToNextAttempt;
    }

    switch (state) {
        case "show":
            reconnectModal.showModal();
            break;
        case "hide":
            reconnectModal.close();
            retryOnVisibility = false;
            break;
        case "failed":
            retryOnVisibility = true;
            break;
        case "retrying":
            retryOnVisibility = false;
            break;
        case "rejected":
            location.reload();
            break;
    }
}

async function retry() {
    retryOnVisibility = false;
    setButtonsDisabled(true);

    try {
        const successful = await Blazor.reconnect();

        if (successful) {
            reconnectModal.close();
        } else {
            // The server is reachable but the circuit is gone (rejected).
            // Attempting resumeCircuit() here is incorrect — resume is for *paused* circuits,
            // not rejected ones. Reload is the only correct recovery path.
             location.reload();
             return;
        }
    } catch {
        retryOnVisibility = true;
    } finally {
        setButtonsDisabled(false);
    }
}

async function resume() {
    setButtonsDisabled(true);

    try {
        const successful = await Blazor.resumeCircuit();
        if (!successful) {
            location.reload();
        } else {
            reconnectModal.close();
        }
    } catch {
        reconnectModal.classList.remove("components-reconnect-paused");
        reconnectModal.classList.add("components-reconnect-resume-failed");
    } finally {
        setButtonsDisabled(false);
    }
}

document.addEventListener("visibilitychange", () => {
    if (retryOnVisibility && document.visibilityState === "visible") {
        retry();
    }
});

function setButtonsDisabled(disabled) {
    if (retryButton)  retryButton.disabled  = disabled;
    if (resumeButton) resumeButton.disabled = disabled;
}