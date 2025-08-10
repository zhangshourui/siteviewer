'use client';

import { App } from 'antd';
import { toast } from 'react-toastify';
import { defaultHttpApiOptions, type MsgRespBase, type ApiRequestOption, apiBase } from './global'

const loadingId = "http-request"
let loadingCounter = 0;

console.log("process.env.NODE_ENV: %s", process.env.NODE_ENV);


export default {
    _baseApiPath: apiBase,
    _defaultOption: defaultHttpApiOptions,

    post: function (apiPath: string, data: object, option: ApiRequestOption = defaultHttpApiOptions) {

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



        let tolerance = tempOption.tolerance || defaultHttpApiOptions.tolerance;
        if (tolerance < -1) {
            tolerance = defaultHttpApiOptions.tolerance;
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