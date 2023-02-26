namespace Camunda.Connector.SDK.Core.Api.Error;

public record BpmnError
{
    public string Code { get; init; }
    public string Message { get; init; }

    public bool HasCode()
    {
        return Code != null;
    }


    //public override bool Equals(object o) {
    //  if (this == o) {
    //    return true;
    //  }
    //  if (o == null || getClass() != o.getClass()) {
    //    return false;
    //  }
    //  BpmnError bpmnError = (BpmnError) o;
    //  return Objects.equals(code, bpmnError.code) && Objects.equals(message, bpmnError.message);
    //}

    //public int hashCode() 
    //{
    //  return Objects.hash(code, message);
    //}

    public override string ToString()
    {
        return "BpmnError{" + "code='" + Code + '\'' + ", message='" + Message + '\'' + '}';
    }
}
