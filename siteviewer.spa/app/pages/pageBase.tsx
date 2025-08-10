'use client';
import { Outlet } from "react-router";

import { useEffect, useState } from "react";
import ToastProvider from "~/components/ToastProvider";
import { Button, message } from 'antd';



export default function PageBase({ children }: { children: React.ReactNode }) {


    useEffect(() => {
        console.debug("PageBase mounted");
    }, []);
    // const [messageApi, contextHolder] = message.useMessage();
    return (
        <>
            <div className="page-base">
                <Outlet />
                <ToastProvider />
            </div>
        </>
    );
}