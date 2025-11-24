import type IInputForEdit from "./IInputForEdit";

export default interface IInputBaseProps extends IInputForEdit {
  editedItem: any;
  updatedHandler: (event: React.FormEvent<HTMLInputElement>) => void;
}
