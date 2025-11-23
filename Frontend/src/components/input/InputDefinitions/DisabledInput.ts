import InputTypesEnum from "../../../enum/InputTypesEnum";
import type IInputDefinition from "../../../interface/IInputDefinition";

export default class DisabledInput implements IInputDefinition {
  name: string;
  typeText: string;
  disabled?: boolean | undefined;

  constructor() {
    this.name = InputTypesEnum.Disabled;
    this.typeText = InputTypesEnum.Text;
  }
}
