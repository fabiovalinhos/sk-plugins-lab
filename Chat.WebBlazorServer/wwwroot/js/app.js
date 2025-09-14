window.scrollToBottom = () => {
    let chat_box = document.getElementById('chatHistory');
    if (chat_box) {
        chat_box.scrollTop = chat_box.scrollHeight;
    }
};