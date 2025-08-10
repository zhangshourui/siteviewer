'use client';


// // components/ToastProvider.jsx
// import { lazy, Suspense } from 'react';
// import { toast } from 'react-toastify';

// const ToastContainer = lazy(() => import('react-toastify').then((mod) => ({ default: mod.ToastContainer })));
// // const toast = lazy(() => import('react-toastify').then((mod) => ({ default: mod.toast })));

// export { toast };


// export default function ToastProvider() {
//   return (
//     <Suspense fallback={null}>
//       <ToastContainer />
//     </Suspense>
//   );
// }



// components/ToastProvider.jsx
import { useEffect, useState } from 'react';
import { ToastContainer,toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

 export { toast };

export default function ToastProvider() {
  const [isMounted, setIsMounted] = useState(false);

  useEffect(() => {
    setIsMounted(true); // 组件挂载后标记为客户端环境
  }, []);

  if (!isMounted) return null; // 服务器端渲染时返回 null

  return <ToastContainer />;
}