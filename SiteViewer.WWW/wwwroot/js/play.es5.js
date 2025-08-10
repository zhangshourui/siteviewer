"use strict";

var video = document.getElementById('videoPlayer');
var url = window.videoUrl;

function loadVideo() {

    if (Hls.isSupported()) {
        (function () {
            var hls = new Hls();
            //  hls.loadSource(url);
            hls.on(Hls.Events.MEDIA_ATTACHED, function () {
                console.log("🎬 视频元素已绑定");
                hls.loadSource(url);
            });

            hls.on(Hls.Events.MANIFEST_PARSED, function () {
                if (window.hasBeenActivated) {
                    video.play();
                }
            });

            hls.on(Hls.Events.ERROR, function (event, data) {
                console.error("❌ HLS.js error:", data);
                if (data.fatal) {
                    switch (data.type) {
                        case Hls.ErrorTypes.NETWORK_ERROR:
                            console.error("⚠️ 网络错误，尝试恢复");
                            hls.startLoad();
                            break;
                        case Hls.ErrorTypes.MEDIA_ERROR:
                            console.error("⚠️ 媒体错误，尝试恢复");
                            hls.recoverMediaError();
                            break;
                        default:
                            console.error("🛑 致命错误，销毁播放器");
                            hls.destroy();
                            break;
                    }
                }
            });

            hls.attachMedia(video);
        })();
    } else if (video.canPlayType('application/vnd.apple.mpegurl')) {
        // For Safari (which natively supports HLS)
        video.src = url;
        video.addEventListener('loadedmetadata', function () {
            video.play();
        });
    } else {
        alert("您的浏览器不支持播放 HLS 视频");
    }
}

loadVideo();
window.hasBeenActivated = false;
// listen window.navigator is actived
window.addEventListener('focus', function () {
    if (!window.hasBeenActivated) {
        if (video.paused) {
            video.play();
        }
    }
    window.hasBeenActivated = true;
});

//video.src = url;
//video.addEventListener('loadedmetadata', () => {
//    video.play();
//});

