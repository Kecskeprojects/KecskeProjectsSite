import { useContext, type JSX } from "react";
import { Navigate } from "react-router-dom";
import { UserContext } from "./Contexts";

export default function ProtectedRoute(props: {
  roles?: Array<string>;
  element?: JSX.Element;
}): JSX.Element | undefined {
  const userContext = useContext(UserContext);

  if (userContext.user?.hasRoles(props.roles)) {
    return props.element;
  }

  return <Navigate to="/" replace />;
}
