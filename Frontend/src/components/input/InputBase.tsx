import type { JSX } from "react";
import InputTypesEnum from "../../enum/InputTypesEnum";
import type IInputBaseProps from "../../interface/IInputBaseProps";
import InputTypeDefinitions from "./InputTypeDefinitions";

export default function InputBase(props: IInputBaseProps): JSX.Element {
  const isHidden = props.isHidden
    ? props.isHidden(props.name, props.editedItem)
    : false;
  if (isHidden) {
    return <></>;
  }

  const isDisabled = props.isDisabled
    ? props.isDisabled(props.name, props.editedItem)
    : false;
  if (isDisabled) {
    return renderInput(InputTypesEnum.Disabled);
  }

  const validationMessage = props.validation
    ? props.validation(props.name, props.editedItem)
    : undefined;

  function innerUpdatedHandler(e: React.InputEvent<HTMLInputElement>) {
    e.preventDefault();

    const key = e.currentTarget.name;
    const value = e.currentTarget.value;

    props.updatedHandler(key, value);
  }

  function renderInput(overrideType?: string): JSX.Element {
    const type = InputTypeDefinitions.getType(overrideType ?? props.inputType);

    if (!type) {
      return <></>;
    }

    return (
      <input
        onInput={innerUpdatedHandler}
        name={props.name}
        type={type.typeText}
        disabled={type.disabled}
        multiple={type.multiple}
      />
    );
  }

  return (
    <div className={"" + (props.className ? ` ${props.className}` : "")}>
      <label htmlFor={props.name}>{props.label}</label>
      {renderInput()}
      {validationMessage ? <span>{validationMessage}</span> : <></>}
    </div>
  );
}
