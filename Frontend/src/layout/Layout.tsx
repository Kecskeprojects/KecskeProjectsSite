import { type JSX } from "react";
import { Link, Outlet } from "react-router-dom";
import ComponentRoleChecker from "../auth/ComponentRoleChecker";

export default function Layout(): JSX.Element {
  return (
    <div className="App">
      <div>
        <ComponentRoleChecker roles={["Admin"]}>
          <Link to={"security"}>Security</Link>
        </ComponentRoleChecker>
        <br />
        Layout
      </div>
      <Outlet />
    </div>
  );
}
