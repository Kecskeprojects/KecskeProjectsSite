import { Outlet } from "react-router-dom";

export default function Layout() {
  return (
    <div className="App">
      <div>Layout</div>
      <Outlet />
    </div>
  );
}
