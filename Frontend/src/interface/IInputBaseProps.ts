import type IInputForEdit from "./IInputForEdit";

export default interface IInputBaseProps extends IInputForEdit {
  editedItem: any;
  updated: (event: React.FormEvent<HTMLInputElement>) => void;
}
