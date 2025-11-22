import { useContext, type JSX } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { UserContext } from "../Contexts";

export type RoleCheckerProps = {
  roles?: Array<string> | undefined;
};

export default function RoleChecker(props: RoleCheckerProps): JSX.Element {
  const userContext = useContext(UserContext);

  if (userContext.user?.hasRoles(props.roles)) {
    return <Outlet />;
  }

  return <Navigate to="/" replace />;
}
