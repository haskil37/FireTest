var audio = document.getElementById("audio");
var volume = document.getElementById("volume");
var volume_button = document.getElementById("volume-button");
var audio_temp = audio.volume;
Audio_Volume();
function Volume_Off() {
    if (audio.volume != 0) {
        audio_temp = audio.volume;
        audio.volume = 0;
        volume.value = 0;
        volume_button.className = "volume-off";
    }
    else {
        if (audio_temp != 0) {
            audio.volume = audio_temp;
            volume.value = audio_temp * 10;
            volume_button.className = "volume";
        }
    }
}
function Audio_Stop() {
    document.getElementById("play-button").className = audio.paused ? "pause" : "play";
    if (audio.paused)
        audio.play();
    else
        audio.pause();
}
function Audio_Volume() {
    volume_button.className = "volume";
    audio.volume = volume.value / 10;
    if (audio.volume == 0)
        volume_button.className = "volume-off";
    audio_temp = audio.volume;
}