import { type RouteConfig, index, layout, route } from "@react-router/dev/routes";

export default [
    layout("pages/pageBase.tsx", [
        index("pages/home/home.tsx"),
        // about page
        //route("list", "pages/home/home.tsx"),
        route("about", "pages/about/about.tsx"),
        route("view", "pages/view/view.tsx"),
    ]),


] satisfies RouteConfig;
