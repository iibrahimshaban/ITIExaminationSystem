function startTimer(seconds) {
    const el = document.getElementById("timer");
    const interval = setInterval(() => {
        let m = Math.floor(seconds / 60);
        let s = seconds % 60;
        el.textContent = `${m}:${s < 10 ? '0' : ''}${s}`;
        seconds--;

        if (seconds < 0) {
            clearInterval(interval);
            document.forms[0].submit();
        }
    }, 1000);
}
