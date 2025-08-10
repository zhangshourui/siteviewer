import { useEffect, useRef, useState } from "react";

import { Alert, Button, Flex } from 'antd';
import { httpRequest } from "~/utils";
import type { MsgRespBase } from "~/utils/httpRequest";
import './play.scss';
import Hls from "hls.js";

interface ViewMsgResp extends MsgRespBase {		// 类型继承

}

export default function Play(props: { file?: string }) {
    const [file, setFile] = useState<string>('');
    const [fileName, setFileName] = useState<string>('');
    const [pageData, setPageData] = useState<any>({});
    const [errorMsg, setErrorMsg] = useState<string>("");
    const videoRef = useRef<HTMLVideoElement>(null);


    useEffect(() => {
        setFile(props.file || '');
    }, []);

    // fetch data on page loaded
    useEffect(() => {
        if (!file) {
            setErrorMsg("File not specified, param 'file' is required.");
            return;
        }
        setFileName(file);

        setErrorMsg("");
        httpRequest.post('/rep/Play', {
            File: file,
        }).then((result: ViewMsgResp) => {
            if (result && result.Result != 0) {
                setErrorMsg(result.ErrorMsg || "Unknown error");
                return;
            }
            setPageData(result?.Data || {});
            if (result.Data && result.Data.VideoUrl) {
                loadVideo(result.Data.VideoUrl);
            }
            else {
                setErrorMsg("No video URL provided in response.");
            }

        }, (errData: any) => {

            //this.setState({ pageData: errData });
        });
    }, [file]);

    function loadVideo(src: string) {
        const video = videoRef.current;
        const hls = new Hls();
        if (!video) {
            return;
        }
        if (Hls.isSupported()) {
            //  hls.loadSource(url);
            hls.on(Hls.Events.MEDIA_ATTACHED, function () {
                console.log("🎬 视频元素已绑定");
                hls.loadSource(src);
            });

            hls.on(Hls.Events.MANIFEST_PARSED, () => {
                video.play();
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


        } else if (video.canPlayType('application/vnd.apple.mpegurl')) {
            // For Safari (which natively supports HLS)
            video.src = src;
            video.addEventListener('loadedmetadata', () => {
                video.play();
            });
        } else {
            alert("您的浏览器不支持播放 HLS 视频");
        }
    }

    return (<div className="play">
        {(errorMsg || false) && <Alert message={errorMsg} type="error" />}
        <div className="row">
            <div className="col">

                <div id="video-container" className="video-player">
                    <h4 className="video-title">{pageData?.FileName || file}</h4>
                    <video id="videoPlayer"
                        preload="auto"
                        controls
                        webkit-playsinline="true" playsInline={true}
                        ref={videoRef}
                    >

                    </video>
                    <div className="actions">

                        <Flex gap="large" wrap>
                            <Button onClick={() => videoRef.current?.play()} >播放</Button>
                            <Button onClick={() => videoRef.current?.requestFullscreen()} >全屏</Button>
                        </Flex>
                    </div>


                </div>
            </div>
        </div>
    </div >
    )
}