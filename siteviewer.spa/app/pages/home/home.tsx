import { Alert, Modal, Dropdown, type MenuProps, Space, Input, Button } from 'antd';
import type { Route } from "../home/+types/home";
import { useLocation, useNavigate, Link } from 'react-router';
import { useEffect, useState } from "react";

import { toast } from "~/components/ToastProvider";

import { httpRequest } from "../../utils";
import RenameFileModal from './renameFileModal';
// import {  message } from 'antd';

import './home.scss'

export default function Home() {
  const [parentDir, setParentDir] = useState<any>('');
  const [pageData, setPageData] = useState<any>({});
  const [errorMsg, setErrorMsg] = useState<string>("");
  const [activeFile, setActiveFile] = useState<any>(null);
  const [modalActiveFile, setModalActiveFile] = useState<any>(null);
  const location = useLocation();
  const navigate = useNavigate();
  // const [messageApi, contextHolder] = message.useMessage();


  // 监听 /?p=xxxx 变化
  useEffect(() => {


    handlePopState(); // 初始化时调用一次

    const handleScroll = () => {
      if (window.scrollY > 0) {
        sessionStorage.setItem('ls-scroll-pos-home', window.scrollY.toString());
      }
    };

    window.addEventListener('scroll', handleScroll);
    //  messageApi.info("欢迎使用 SiteViewer");

    setTimeout(() => {

    }, 2000);
    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, []);



  // fetch data on page loaded
  useEffect(() => {
    if (!parentDir) {
      return;
    }
    setErrorMsg("");
    httpRequest.post('/rep/files', {
      ParentDir: parentDir,
    }).then((result: any) => {
      if (result && result.Result != 0) {
        setErrorMsg(result.ErrorMsg || "Unknown error");
        return;
      }
      let files = result?.Files || [];

      var cookies = document.cookie.split(';');
      files.forEach(function (file: any) {
        //cookie中存在`fv-${file.FileId}，则标记为已访问
        if (cookies.some(cookie => cookie.trim().startsWith(`fv-${file.FileId}=`))) {
          file.visited = true;
        }

      });

      setPageData(result || {});

    }, (errData: any) => {

      //this.setState({ pageData: errData });
    });
  }, [parentDir]);

  useEffect(() => {
    if (pageData) {
      requestAnimationFrame(() => {
        const savedPosition = sessionStorage.getItem('ls-scroll-pos-home');
        if (savedPosition) {
          window.scrollTo(0, parseInt(savedPosition));
        }
      });
    }
  }, [pageData]);

  const handlePopState = () => {
    const urlParams = new URLSearchParams(window.location.search);
    const p = urlParams.get('p') || urlParams.get('parentDir') || '/';
    setParentDir(p);
  };


  /**
   * 去掉时分秒
   * @param dateTime 
   * @returns 
   */
  const formatFileDateTime = (dateTime: string) => {
    return new Date(dateTime).toLocaleDateString();
  };

  const onViewDir = (e: React.MouseEvent<HTMLAnchorElement, MouseEvent>, dir: any) => {
    // if (e) {
    //   e.preventDefault();
    //   e.stopPropagation();
    // }

    setErrorMsg("");
    sessionStorage.setItem('ls-scroll-pos-home', '0');
    setParentDir(dir?.Link ?? '/');

  };

  const onViewFile = (e: React.MouseEvent<HTMLAnchorElement, MouseEvent>, file: any) => {
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }
    if (!file) {
      return;
    }

    setErrorMsg("");

    httpRequest.post('/rep/view', {
      file: file.Link,
    }).then((result: any) => {
      if (result && result.Result != 0) {
        setErrorMsg(result.ErrorMsg || "Unknown error");
        return;
      }
      document.cookie = `fv-${file.FileId}=1; max-age=${24 * 60 * 60 * 7}; path=/`;
      file.visited = true;
      setPageData(result?.Data || {});


      if (result.Data.Action == "view") {// 浏览器支持的格式，直接跳转
        window.location.href = result.Data.FileUrl;
      }
      else if (result.Data.Action == "play") {// 视频格式，跳转到播放页面
        //  route(`/view/play?p=${result.Data.Path}`);
        //window.location.href = `/view?action=play&p=${encodeURIComponent(result.Data.Path)}`;
        navigate(`/view?action=play&p=${encodeURIComponent(result.Data.Path)}`, { state: { scrollY: window.scrollY } });
      } else {
        window.location.href = result.Data.FileUrl;
        // setErrorMsg("Unsupported file type");
      }

    });
  };

  const onFileRenamed = (file: any, srcFileId: string) => {
    setModalActiveFile(null);
    // 用file替换pageData.Files中的对应文件
    const index = pageData.Files.findIndex((f: any) => f.FileId === srcFileId);
    if (index !== -1) {
      pageData.Files[index] = { ...file }; // 替换文件信息
    }
    setPageData({ ...pageData });
    // Handle rename logic here
  }


  var subDirectories = pageData?.Directories
  var subFiles = pageData?.Files

  const renderDirectoryItem = (subDir: any, index: number) => {
    return (
      <li className="dir" key={index} >
        <Link to={`/?p=${encodeURIComponent(subDir.Link)}`} className='dir-link'
          onClick={(e) => {
            onViewDir(e, subDir);
          }}>
          <img src="/images/icons/directory.svg" className="icon-dir" />
          <span className='ms-1'>
            {subDir.DirectoryName}
          </span>
        </Link>
        <div className='file-opt'>
          <span className='file-info ms-2'> {formatFileDateTime(subDir.LastTime)}</span>
        </div>
      </li>
    )
  }

  const renderFileItem = (file: any, index: number) => {

    const menuItems: MenuProps['items'] = [
      {
        key: '1',
        label: (
          <span>
            Rename...
          </span>
        ),
        onClick: () => {
          if (!activeFile) {
            return;
          }
          setModalActiveFile({ ...activeFile });

        }
      },

    ];
    return (
      <li className={'file '} key={index} >

        <Link to={`/view?p=${file.Link}`}
          data-id={file.FileId}
          className={'detail-link ' + (file.visited ? 'visited' : '')}
          onClick={(e) => {
            onViewFile(e, file);
          }}
        >
          {file.FileName}
        </Link>

        <div className='file-opt'>
          <span className='file-info ms-2'>{file.FileSizeReadable}<br /> {formatFileDateTime(file.LastTime)}</span>

          <Dropdown menu={{ items: menuItems }} trigger={['click']} className='pop-menu'
            onOpenChange={(open) => {
              if (open) {
                setActiveFile(file);
              }
            }}
          >
            <a onClick={(e) => e.preventDefault()} className='pop-menu-btn'>
              <Space>
                <img src="/images/icons/more-vert.svg" className="icon-dir" />
                {/* a
                        <DownOutlined /> */}
              </Space>
            </a>
          </Dropdown>
          {/* <a href='' onClick={(e) => { e.preventDefault(); e.stopPropagation(); }} >
                  </a> */}
        </div>
      </li>)
  }

  return (
    <>
      {(errorMsg || false) && <Alert message={errorMsg} type="error" />}

      <div className="home">

        <h5 className="dir-head">
          <span className="dir-actions">
            {
              pageData?.ParentDirectoryLink && <Link to={`/?p=${encodeURIComponent(pageData.ParentDirectoryLink)}`}
                onClick={(e) => {
                  onViewDir(e, { Link: pageData.ParentDirectoryLink });
                }}
              >
                ↑向上
              </Link>
            }
          </span>
          <span>{parentDir == "/" ? "/根目录" : parentDir}</span>
        </h5>


        <div className="res-list -container">
          <ul className="res-list ">
            {subDirectories?.map((subDir: any, index: number) => renderDirectoryItem(subDir, index))}
          </ul>
        </div >

        <div className="res-list-container">
          <ul className="res-list">
            {subFiles?.map((file: any, index: number) => renderFileItem(file, index))}

          </ul>
        </div>
      </div>

      <RenameFileModal
        modalActiveFile={modalActiveFile}
        onRenameOk={onFileRenamed}
        onRenameCancel={() => { setModalActiveFile(null); }}
      />

    </>
  );
}

