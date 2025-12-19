import { useContext } from "react";
import type IComponentRoleCheckerProps from "../interface/IComponentRoleCheckerProps";
import { UserContext } from "../utilities/Contexts";

export default function ComponentRoleChecker(
  props: IComponentRoleCheckerProps
): React.ReactNode {
  const userContext = useContext(UserContext);

  if (userContext.user?.hasRoles(props.roles)) {
    return props.children;
  }

  return <></>;
}
