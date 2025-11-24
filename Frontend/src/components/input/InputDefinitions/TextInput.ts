import InputTypesEnum from "../../../enum/InputTypesEnum";
import type IInputDefinition from "../../../interface/IInputTypeDefinition";

export default class TextInput implements IInputDefinition {
  name: string;
  typeText: string;

  constructor() {
    this.name = InputTypesEnum.Text;
    this.typeText = InputTypesEnum.Text;
  }
}
