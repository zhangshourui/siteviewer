import { useEffect, useState } from "react";
import { Alert } from 'antd';
import { useLocation, Link } from 'react-router';
import { httpRequest } from "../../utils";
import Play from './play';

import './view.scss'

export default function View(/* props: { file?: string , action: 'play'|null|undefined} */) {
  const [file, setFile] = useState<any>('');
  const [action, setAction] = useState<any>('none');
  const [pageData, setPageData] = useState<any>({});
  const [errorMsg, setErrorMsg] = useState<string>("");
  const location = useLocation();

  // 监听 /?p=xxxx 变化
  useEffect(() => {

    const urlParams = new URLSearchParams(window.location.search);
    const p = urlParams.get('p');
    setFile(p);
    setAction(urlParams.get('action') || 'none');

    window.addEventListener('popstate', handlePopState);
    handlePopState(); // 初始化时调用一次

    return () => {
      window.removeEventListener('popstate', handlePopState);
    };

  }, []);

  const handlePopState = () => {
    if (location.state?.scrollY !== undefined) {
      window.scrollTo(0, location.state.scrollY);
    }
  };

  useEffect(() => {
    if (!action || action == 'none') {
      updateFileViewInfo();
    }
  }, [file]);


  //   // fetch data on page loaded
  const updateFileViewInfo = () => {
    if (!file) {
      return;
    }
    setErrorMsg("");
    httpRequest.post('/rep/view', {
      file: file,
    }).then((result: any) => {
      if (result && result.Result != 0) {
        setErrorMsg(result.ErrorMsg || "Unknown error");
        return;
      }
      setAction(result.Data.Action || 'none');

    }, (errData: any) => {

      //this.setState({ pageData: errData });
    });
  }


  return (
    <>
      {(errorMsg || false) && <Alert message={errorMsg} type="error" />}


      <div>
        {action == "play" && (
          <Play file={file} />
        )}
      </div >


    </>
  );
}
