window.scrollToBottom = () => {
    window.scrollTo(0, document.body.scrollHeight);
}

window.initEnterKeyListener = (inputElement, dotNetObject) => {
    inputElement.onkeydown = async (e) => {
        if ((e.code === "Enter" || e.code === "NumpadEnter") && !e.shiftKey && !e.ctrlKey && !e.altKey) {
            e.preventDefault();
            await dotNetObject.invokeMethodAsync('SendMessageFromJS');
            inputElement.focus();
        }
    };
};