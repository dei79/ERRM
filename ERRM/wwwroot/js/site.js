// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    // Only activate the streaming logic on the report page.
    const streamingRoot = document.getElementById("report-generation");
    if (!streamingRoot) {
        return;
    }

    // "dataset.streamUrl" reads the HTML attribute "data-stream-url" from the report container.
    // That attribute is written by the Razor view and gives JavaScript the backend URL to call for streaming.
    const streamUrl = streamingRoot.dataset.streamUrl;
    const statusElement = document.getElementById("report-status");
    const errorElement = document.getElementById("report-error");
    const reportElement = document.getElementById("rendered-report");
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!streamUrl || !statusElement || !errorElement || !reportElement || !antiForgeryToken) {
        return;
    }

    reportElement.textContent = "";
    void streamReport(streamUrl, antiForgeryToken, statusElement, errorElement, reportElement);
});

async function streamReport(streamUrl, antiForgeryToken, statusElement, errorElement, reportElement) {
    try {
        // Start the server request that streams the generated report back chunk by chunk.
        const response = await fetch(streamUrl, {
            method: "POST",
            headers: {
                RequestVerificationToken: antiForgeryToken,
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        if (!response.ok || !response.body) {
            throw new Error("The report stream could not be started.");
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = "";

        while (true) {
            // Read the next binary chunk from the HTTP stream.
            const { done, value } = await reader.read();
            if (done) {
                break;
            }

            // Convert bytes to text and append them to the local buffer.
            buffer += decoder.decode(value, { stream: true });

            // The backend sends NDJSON: one JSON object per line.
            // We keep reading until we have a complete line, then parse and handle it.
            let newlineIndex = buffer.indexOf("\n");
            while (newlineIndex >= 0) {
                const line = buffer.slice(0, newlineIndex).trim();
                buffer = buffer.slice(newlineIndex + 1);

                if (line) {
                    processReportUpdate(line, statusElement, errorElement, reportElement);
                }

                newlineIndex = buffer.indexOf("\n");
            }
        }

        if (buffer.trim()) {
            processReportUpdate(buffer.trim(), statusElement, errorElement, reportElement);
        }
    } catch (error) {
        statusElement.classList.remove("alert-info");
        statusElement.classList.add("alert-danger");
        statusElement.textContent = "Report generation failed.";
        errorElement.classList.remove("d-none");
        errorElement.textContent = error instanceof Error
            ? error.message
            : "An unexpected streaming error occurred.";
    }
}

function processReportUpdate(line, statusElement, errorElement, reportElement) {
    // Convert one streamed JSON line into a JavaScript object.
    const update = JSON.parse(line);
    const type = update.type ?? update.Type;
    const content = update.content ?? update.Content ?? "";
    const errorMessage = update.errorMessage ?? update.ErrorMessage ?? "An unexpected streaming error occurred.";

    if (type === "content") {
        // Append the next text chunk to the visible report.
        reportElement.textContent += content;
        return;
    }

    if (type === "complete") {
        // The backend signals that no more chunks will follow.
        statusElement.classList.remove("alert-info");
        statusElement.classList.add("alert-success");
        statusElement.textContent = "Report generation completed.";
        return;
    }

    if (type === "error") {
        // Show a user-friendly message if the stream reports an error.
        statusElement.classList.remove("alert-info");
        statusElement.classList.add("alert-danger");
        statusElement.textContent = "Report generation failed.";
        errorElement.classList.remove("d-none");
        errorElement.textContent = errorMessage;
    }
}
