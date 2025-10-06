// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

public class CT_Counter : CounterTop
{
    /// <summary>
    /// Represents a subclass of the CounterTop class called CT_Counter.
    /// Implements the Interact() method from base CounterTop. This is the base counter, which is used for placing objects on.
    /// </summary>



    public override InteractionResponse Interact(KitchenObject inKitchenObject = null)
    {
        // Call the parent class' Interact() method
        return base.Interact(inKitchenObject);
    }
}
