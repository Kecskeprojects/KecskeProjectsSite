import type { JSX } from "react";
import { Outlet } from "react-router-dom";

export default function Layout(): JSX.Element {
  return (
    <div className="App">
      <div>Layout</div>
      <Outlet />
    </div>
  );
}
