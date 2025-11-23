import InputTypesEnum from "../../../enum/InputTypesEnum";
import type IInputDefinition from "../../../interface/IInputDefinition";

export default class PasswordInput implements IInputDefinition {
  name: string;
  typeText: string;

  constructor() {
    this.name = InputTypesEnum.Password;
    this.typeText = InputTypesEnum.Password;
  }
}
