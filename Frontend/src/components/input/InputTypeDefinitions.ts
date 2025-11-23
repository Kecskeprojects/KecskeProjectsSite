import InputTypesEnum from "../../enum/InputTypesEnum";

export type InputTypeParameters = {
  name: string;
  typeText: string;
  disabled?: boolean;
};
//https://www.w3schools.com/tags/tag_input.asp
export default class InputTypeDefinitions {
  static getType(typeName: string) {
    return InputTypeDefinitions.Types.find((x) => x.name === typeName);
  }

  static Types = new Array<InputTypeParameters>(
    { name: InputTypesEnum.Text, typeText: InputTypesEnum.Text },
    {
      name: InputTypesEnum.Disabled,
      typeText: InputTypesEnum.Text,
      disabled: true,
    }
  );
}
