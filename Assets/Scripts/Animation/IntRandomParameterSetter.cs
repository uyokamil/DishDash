// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

// This class is responsible for setting an integer parameter randomly in a Unity Animator's state machine behaviour.
// It inherits from the StateMachineBehaviour class and overrides the OnStateEnter method to perform the random parameter setting.
// The class has two public fields: IntParameterName (the name of the integer parameter to be set randomly) and StateCount (the number of states for the integer parameter).
// When the state is entered, the OnStateEnter method is called and it sets the integer parameter to a random value between 0 and StateCount using the animator.SetInteger method.
// Additionally, it sets the "RandomSpeed" float parameter to a random value between 0.2 and 1.8 using the animator.SetFloat method.
// The random speed set in this context determines the playback speed of the IDLE animation, adding variation to the animation playback so the NPC characters nod/shake their heads at different times.

public class IntParameterRandomSetter : StateMachineBehaviour
{
    // The name of the integer parameter to be set randomly.
    public string IntParameterName;

    // The number of states for the integer parameter.
    public int StateCount = 0;

    // Called when the state is entered.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Set the integer parameter to a random value between 0 and StateCount.
        animator.SetInteger(IntParameterName, Random.Range(0, StateCount));

        // Set the "RandomSpeed" float parameter to a random value between 0.2 and 1.8.
        animator.SetFloat("RandomSpeed", Random.Range(0.2f, 1.8f));
    }
}
