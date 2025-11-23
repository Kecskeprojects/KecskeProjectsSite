import type { JSX } from "react";
import type { IWindowDescription } from "../../interface/IWindowDescription";

export type EditWindowBaseProps = {
  windowDescription: IWindowDescription;
};

export default function EditWindowBase(
  props: EditWindowBaseProps
): JSX.Element {
  return (
    <div>
      <h1>{props.windowDescription.title}</h1>
      <form></form>
    </div>
  );
}
