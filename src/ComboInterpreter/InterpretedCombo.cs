namespace ComboInterpreter;

public record class InterpretedCombo
{
    public required string DisplayName { get; set; }

    public SimpleButtons Buttons { get; set; }

    public bool HasContinuation { get; set; } = false;

    public bool EndsCombo { get; set; } = false;

    public required ActionEvent ActionEvent { get; set; }

    public int Frame => ActionEvent.Frame;
}
