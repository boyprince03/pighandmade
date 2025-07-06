function saveChatHistory(html) {
    localStorage.setItem("chatbot-history", html);
}

function loadChatHistory() {
    return localStorage.getItem("chatbot-history") || "";
}

document.addEventListener("DOMContentLoaded", function () {
    const chatbotIcon = document.getElementById("chatbot-icon");
    const chatbotTooltip = document.getElementById("chatbot-tooltip");
    const chatbotWindow = document.getElementById("chatbot-window");
    const chatbotBody = document.getElementById("chatbot-body");
    const chatbotInput = document.getElementById("chatbot-input");
    // 載入歷史紀錄
    const history = loadChatHistory();
    if (history && history.trim().length > 0) {
        chatbotBody.innerHTML = history;
    } else {
        chatbotBody.innerHTML = loadDefaultFAQ();
    }

    let lastTooltipTime = 0;

    // 滑過圖示顯示歡迎詞
    chatbotIcon.addEventListener("mouseenter", function () {
        const now = Date.now();
        if (now - lastTooltipTime < 10000) return;
        lastTooltipTime = now;
        document.getElementById("chat-role").textContent = window.userRole || "訪客";
        document.getElementById("chat-username").textContent = window.userName || "朋友";

        chatbotTooltip.classList.add("show");
        chatbotTooltip.classList.remove("hide");

        setTimeout(() => {
            chatbotTooltip.classList.remove("show");
            chatbotTooltip.classList.add("hide");
        }, 3000);
    });

    // 攔截回應中超連結
    chatbotBody.addEventListener("click", function (e) {
        if (e.target.classList.contains("chatbot-link")) {
            e.preventDefault();
            const url = e.target.getAttribute("href");
            window.location.href = url;
        }
    });

    // 切換 chatbot 視窗
    window.toggleChatbot = function () {
        if (chatbotWindow.classList.contains("visible")) {
            chatbotWindow.classList.remove("visible");
            chatbotWindow.classList.add("hidden");
        } else {
            chatbotWindow.classList.remove("hidden");
            chatbotWindow.classList.add("visible");
            chatbotTooltip.classList.remove("show");
            chatbotTooltip.classList.add("hide");
        }
    };

    // 關閉視窗
    window.closeChatbot = function () {
        chatbotWindow.classList.remove("visible");
        chatbotWindow.classList.add("hidden");
        clearChatHistory();
    };

    // 預設常見問題（點選自動帶入並送出）
    window.sendPredefined = function (message) {
        chatbotInput.value = message;
        sendMessage();
    };

    // 發送對話（統一API: /api/chatbot/ask）
    window.sendMessage = async function () {
        const msg = chatbotInput.value.trim();
        if (!msg) return;

        appendMessage("你", msg, "user");
        chatbotInput.value = "";

        // Loading
        appendMessage("AI", "思考中...", "ai", true);

        try {
            const res = await fetch("/api/chatbot/ask", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ message: msg })
            });
            const data = await res.json();
            removeLastBotMessage();
            appendMessage("AI", data.reply, "ai");
        } catch (err) {
            removeLastBotMessage();
            appendMessage("AI", "❌ 伺服器錯誤，請稍後再試", "ai");
        }
        chatbotBody.scrollTop = chatbotBody.scrollHeight;
        // 儲存對話
        saveChatHistory(chatbotBody.innerHTML);
    };
});

function clearChatHistory() {
    localStorage.removeItem("chatbot-history");
    const body = document.getElementById("chatbot-body");
    body.innerHTML = loadDefaultFAQ();
}

function loadDefaultFAQ() {
    return `
        <div><strong>常見問題：</strong></div>
        <ul>
            <li onclick="sendPredefined('如何查詢訂單？')">如何查詢訂單？</li>
            <li onclick="sendPredefined('如何申請退貨？')">如何申請退貨？</li>
            <li onclick="sendPredefined('我要修改密碼')">我要修改密碼</li>
        </ul>
    `;
}

// 訊息顯示
function appendMessage(sender, text, type, isLoading = false) {
    const chatBody = document.getElementById("chatbot-body");
    const div = document.createElement("div");
    div.className = type === "user" ? "chat-user" : "chat-ai";
    div.innerHTML = `<strong>${sender}：</strong><span>${isLoading ? "<em>..." : escapeHTML(text)}</span>`;
    chatBody.appendChild(div);
    chatBody.scrollTop = chatBody.scrollHeight;
}

function removeLastBotMessage() {
    const chatBody = document.getElementById("chatbot-body");
    const msgs = chatBody.querySelectorAll(".chat-ai");
    if (msgs.length > 0) chatBody.removeChild(msgs[msgs.length - 1]);
}

// 防止XSS
function escapeHTML(str) {
    return str.replace(/[&<>"']/g, function (m) {
        return {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        }[m];
    });
}
