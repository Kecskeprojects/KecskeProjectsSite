import type IWindowDescription from "./IWindowDescription";

export default interface IEditWindowBaseProps<
  windowDescriptionType extends IWindowDescription
> {
  windowDescriptionClass: { new (): windowDescriptionType };
  responseHandler: (content?: any) => void;
}
