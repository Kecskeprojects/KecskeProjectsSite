import type { JSX } from "react";
import type { IWindowDescription } from "./WindowDescriptions/IWindowDescription";

export type EditWindowBaseProps = {
  windowDescription: IWindowDescription;
};

export default function EditWindowBase(
  props: EditWindowBaseProps
): JSX.Element {
  return <div>{props.windowDescription.title}</div>;
}
