'use client';

import { App } from 'antd';
import { toast } from 'react-toastify';
const loadingId = "http-request"
let loadingCounter = 0;


interface ApiRequestOption {
    /**
     * 响应等待容忍度。如果此时间内没有响应，则显示loading框。
     * 0表示总是显示，-1表示不显示。单位毫秒
     */
    tolerance: number;

    /**
     * 发生错误是否通过toast显示错误。默认显示（false），不显示（静默）为true
     */
    silence: boolean;

    noErrorToast: boolean;

    headers: any[]; // 或者更具体的类型，如果知道 headers 的结构
}

const DEFAULT_OPTION: ApiRequestOption = {
    tolerance: 500,
    silence: false,
    noErrorToast: false,
    headers: []
};

let apiBase = 'http://192.168.1.140:8082/api';
//debug
if (process.env.NODE_ENV === 'development') {
    apiBase = 'https://localhost:7150/api';
}
console.log("process.env.NODE_ENV: %s", process.env.NODE_ENV);
//const [messageApi, contextHolder] = message.useMessage();

// const { message: messageApi } = App.useApp();

interface MsgRespBase {
    Result: number //, Action: string, Path: string,
    ErrorMsg?: string;
    Data?: any;
}
export {
    DEFAULT_OPTION
};
export type { MsgRespBase };

export default {
    _baseApiPath: apiBase,
    _defaultOption: DEFAULT_OPTION,

    post: function (apiPath: string, data: object, option: ApiRequestOption = DEFAULT_OPTION) {

        let tempOption = {
            ...this._defaultOption,
            ...option
        };
        let isComplete = false;
        let isShowLoading = false;

        if (!apiPath.startsWith("http")) {

            if (!apiPath.startsWith('/')) {
                apiPath = "/" + apiPath;
            }
            apiPath = this._baseApiPath + apiPath;
        }



        let tolerance = tempOption.tolerance || DEFAULT_OPTION.tolerance;
        if (tolerance < -1) {
            tolerance = DEFAULT_OPTION.tolerance;
        }
        if (!tempOption.silence) {
            setTimeout(() => {
                if (!isComplete) {
                    toast.loading("加载中...", {
                        toastId: loadingId
                    });
                    isShowLoading = true;
                    loadingCounter++;
                    // console.log("loading count: " + loadingCounter);
                }
            }, tolerance)
        }
        return new Promise((resolve: (value: MsgRespBase) => void, reject: (reason?: MsgRespBase) => void) => {
            let headers = {
                //'Content-Type': 'application/x-www-form-urlencoded',
                'Content-Type': 'application/json',
            };
            if (tempOption.headers) {
                headers = {
                    ...headers,
                    ...tempOption.headers
                }
            }
            let bodyContent = "";
            if (headers["Content-Type"] == "application/x-www-form-urlencoded" && typeof data === 'object') {

                bodyContent = toQueryString(data);
            } else {
                bodyContent = JSON.stringify(data);;
            }


            fetch(apiPath, {
                credentials: 'include',
                method: 'post',
                headers: headers,
                body: bodyContent
            }).then((response) => {
                isComplete = true;
                if (isShowLoading) {
                    loadingCounter--;
                    if (loadingCounter == 0) {
                        toast.dismiss(loadingId);

                    }
                }
                return response.json()
            }).then((result) => {
                if (!result.errMsg) {
                    return resolve(result);
                } else {
                    if (!tempOption.silence && !tempOption.noErrorToast) {

                        toast.error(result.errMsg, {
                            position: "top-center",
                            autoClose: 3000,
                            hideProgressBar: true,
                            closeOnClick: false,
                            pauseOnHover: true,
                            draggable: false,
                            progress: undefined,
                        });
                        console.log('error: %o', result.errMsg);
                    }
                    return reject(result);
                }

            }).catch(function (error) {
                console.log('request: %s, result error: %o', apiPath, error)

                isComplete = true;
                if (isShowLoading) {
                    loadingCounter--;
                    if (loadingCounter == 0) {
                        toast.dismiss(loadingId);

                    }
                }

                return reject({ Result: -1, ErrorMsg: JSON.stringify(error) });
            })
        });

    }

}

function toQueryString(obj: { [key: string]: any }) {

    let query = ""
    for (let i in obj) {
        let value = obj[i];
        if (Array.isArray(value)) {
            value = value.join(",")
        }
        query += `&${i}=${value}`
    }
    if (query) {
        query = query.substring(1);
    }
    return query

}