import type IWindowInput from "./IWindowInput";

export default interface IInputBaseProps extends IWindowInput {
  editedItem: any;
  updated: (event: React.FormEvent<HTMLInputElement>) => void;
}
