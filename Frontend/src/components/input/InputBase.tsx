import type { JSX } from "react";
import InputTypesEnum from "../../enum/InputTypesEnum";
import type IInputBaseProps from "../../interface/IInputBaseProps";
import InputTypeDefinitions from "./InputTypeDefinitions";

/**
 * TODO: Frontend Input Validation (Security Recommendation)
 * Both frontend and backend validation required for security
 *
 * 1. EMAIL VALIDATION
 *    - Use HTML5 pattern attribute or regex: /^[^\s@]+@[^\s@]+\.[^\s@]+$/
 *    - Validate before form submission
 *
 * 2. PASSWORD STRENGTH VALIDATION (especially for registration)
 *    - Minimum 12 characters
 *    - Must contain uppercase letter (A-Z)
 *    - Must contain lowercase letter (a-z)
 *    - Must contain number (0-9)
 *    - Must contain special character (!@#$%^&*(),.?":{}|<>)
 *    - Display validation errors/requirements in real-time
 *    - Consider adding a password strength meter component
 *
 * 3. FILE SIZE VALIDATION
 *    - Use getFileSizeLimit() from EnvironmentTools
 *    - Validate file size before upload
 *    - Show error message if file exceeds limit
 *
 * 4. STRING LENGTH LIMITS
 *    - Username: Max 50 characters
 *    - File names: Max 255 characters
 *    - Any text field: Apply reasonable maxlength
 *
 * 5. IMPLEMENTATION NOTES
 *    - Use HTML5 attributes first (pattern, maxlength, required)
 *    - Add JavaScript validation for better UX (show errors before submit)
 *    - Backend validation is CRITICAL - never trust frontend alone
 *    - React automatically escapes content in JSX (safe for display)
 *
 * See SECURITY_RECOMMENDATIONS.md section 8 for backend password policy implementation
 */

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
