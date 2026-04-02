function showRealtimeToast(title, message) {
    const host = document.getElementById("realtimeToastHost");
    if (!host) return;

    const wrapper = document.createElement("div");
    wrapper.className = "toast toast-soft";
    wrapper.setAttribute("role", "alert");
    wrapper.setAttribute("aria-live", "assertive");
    wrapper.setAttribute("aria-atomic", "true");
    wrapper.innerHTML = `
        <div class="toast-header border-0 bg-transparent">
            <strong class="me-auto">${title}</strong>
            <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
        </div>
        <div class="toast-body">${message}</div>
    `;

    host.appendChild(wrapper);
    const toast = new bootstrap.Toast(wrapper, { delay: 5000 });
    toast.show();
    wrapper.addEventListener("hidden.bs.toast", () => wrapper.remove());
}

document.addEventListener("DOMContentLoaded", () => {
    const appShell = document.getElementById("appShell");
    const sidebarToggles = document.querySelectorAll("[data-sidebar-toggle]");

    if (appShell && sidebarToggles.length > 0) {
        const setSidebarState = (isOpen) => {
            appShell.classList.toggle("is-sidebar-open", isOpen);
            document.body.classList.toggle("sidebar-open", isOpen);
        };

        setSidebarState(false);

        sidebarToggles.forEach((button) => {
            button.addEventListener("click", () => {
                setSidebarState(!appShell.classList.contains("is-sidebar-open"));
            });
        });

        document.querySelectorAll(".nav-item").forEach((item) => {
            item.addEventListener("click", () => setSidebarState(false));
        });

        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape") {
                setSidebarState(false);
            }
        });

        window.addEventListener("resize", () => {
            if (window.innerWidth < 992) {
                setSidebarState(false);
            }
        }, { passive: true });
    }

    document.querySelectorAll("form[data-confirm]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            const message = form.dataset.confirm;
            if (message && !window.confirm(message)) {
                event.preventDefault();
            }
        });
    });

    if (window.signalR && document.querySelector(".app-shell")) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/budgetHub")
            .withAutomaticReconnect()
            .build();

        connection.on("budgetAlert", (payload) => {
            const message = `${payload.message} (${payload.spentAmount.toLocaleString()} / ${payload.limitAmount.toLocaleString()} VND)`;
            showRealtimeToast("Canh bao ngan sach", message);
        });

        connection.on("walletAlert", (payload) => {
            const message = `${payload.message} So du hien tai: ${payload.currentBalance.toLocaleString()} VND.`;
            showRealtimeToast("Canh bao so du vi", message);
        });

        connection.start().catch(() => {
            console.warn("Khong ket noi duoc SignalR.");
        });
    }
});
