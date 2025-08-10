import { Modal, Input, Button, message, Space } from 'antd';


import { useEffect, useState } from "react";
// import httpRequest from "../utils/httpRequest";
import { httpRequest } from "../../utils";

export default function RenameFileModal(props: { modalActiveFile: any, onRenameOk?: (file: any, srcFileId: string) => void, onRenameCancel?: () => void }) {
  const { modalActiveFile } = props;
  const [editFileName, setEditFileName] = useState<string>("");
  const [editFileExt, setEditFileExt] = useState<string>("");
  const [messageApi, contextHolder] = message.useMessage();
  const onOk = () => {

    if (!modalActiveFile) {
      return;
    }
    modalActiveFile.FileName = `${editFileName}.${editFileExt}`;
    let srcFileId = modalActiveFile.FileId;
    httpRequest.post('/rep/rename', {
      File: modalActiveFile.Link,
      NewFileName: modalActiveFile.FileName,
    }).then((result: any) => {
      if (result && result.Result != 0) {
        // 处理重命名失败的情况
        messageApi.error(result.ErrorMsg || "重命名失败");
      }
      else {
        messageApi.success("重命名成功");
        if (props.onRenameOk) {
          props.onRenameOk(result.Data.File, srcFileId);
        }
      }

    });

    // delete modalActiveFile.ext;
    // delete modalActiveFile.fileNameWithoutExt;
  }

  const onCancel = () => {
    if (props.onRenameCancel) {
      props.onRenameCancel();
    }
  }

  const onOpenModal = () => {
    if (!modalActiveFile) {
      return;
    }

    // 提取/拆分文件名和扩展名
    const fileNameParts = modalActiveFile.FileName.split('.');
    setEditFileExt(fileNameParts.length > 1 ? fileNameParts.pop() : '');
    setEditFileName(fileNameParts.join('.'));
  }

  const setPrefixName = (prefix: string) => {
    // 判断是否以a{1,}-* 开头， 如果是，则替换为prefix；否则，直接添加前缀prefix
    if (modalActiveFile) {
      if (editFileName.match(/^a{1,}-/)) {
        setEditFileName(editFileName.replace(/^a{1,}-/, prefix ? (prefix + '-') : ''))
      } else {
        setEditFileName((prefix ? (prefix + '-') : '') + editFileName);
      }
    }

  }

  return (
    <>
      {contextHolder}
      <Modal
        title="Rename File"
        open={!!modalActiveFile}
        onOk={onOk}
        onCancel={onCancel}
        afterOpenChange={(open) => { onOpenModal(); }}
      >
        <p>File: {modalActiveFile?.FileName}</p>
        <Space direction="vertical" style={{ width: '100%' }}>

          <div>

            <Input
              value={editFileName}
              style={{ width: '100%' }}
              addonAfter={<span>.{editFileExt}</span>}
              onChange={(e) => {
                if (modalActiveFile) {
                  setEditFileName(e.target.value);
                }
              }}
            />
            {/* <Input style={{ width: '20%' }} value={modalActiveFile?.ext} /> */}
            {/* <Button type="primary"></Button> */}
          </div>
          <div>
            <Space>
              <Button size="small" color="primary" variant="outlined" onClick={() => setPrefixName('a')} >a</Button>
              <Button size="small" color="primary" variant="outlined" onClick={() => setPrefixName('aa')} >aa</Button>
              <Button size="small" color="primary" variant="outlined" onClick={() => setPrefixName('aaaa')} >aaaa</Button>
              <Button size="small" color="primary" variant="outlined" onClick={() => setPrefixName('aaaaa')} >aaaaa</Button>
              <Button size="small" color="danger" variant="outlined" onClick={() => setPrefixName('')} >Clear</Button>
            </Space>
          </div>
        </Space>
      </Modal>
    </>
  );
}
