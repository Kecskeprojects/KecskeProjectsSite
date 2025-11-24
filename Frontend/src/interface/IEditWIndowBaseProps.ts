import type IWindowDescription from "./IWindowDescription";

export default interface EditWindowBaseProps<
  windowDescriptionType extends IWindowDescription
> {
  windowDescriptionClass: { new (): windowDescriptionType };
  responseHandler: (content?: any) => void;
}
