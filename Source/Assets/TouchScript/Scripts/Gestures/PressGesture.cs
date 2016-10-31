/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Utils.Attributes;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes when an object is touched.
    /// Works with any gesture unless a Delegate is set.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Press Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_PressGesture.htm")]
    public class PressGesture : Gesture
    {
        #region Constants

        /// <summary>
        /// Message name when gesture is recognized
        /// </summary>
        public const string PRESS_MESSAGE = "OnPress";

        #endregion

        #region Events

        /// <summary>
        /// Occurs when gesture is recognized.
        /// </summary>
        public event EventHandler<EventArgs> Pressed
        {
            add { pressedInvoker += value; }
            remove { pressedInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<EventArgs> pressedInvoker;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets a value indicating whether actions coming from children should be ingored.
        /// </summary>
        /// <value> <c>true</c> if actions from children should be ignored; otherwise, <c>false</c>. </value>
        public bool IgnoreChildren
        {
            get { return ignoreChildren; }
            set { ignoreChildren = value; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        [ToggleLeft]
        private bool ignoreChildren = false;

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        public override bool ShouldReceivePointer(Pointer pointer)
        {
            if (!IgnoreChildren) return base.ShouldReceivePointer(pointer);
            if (!base.ShouldReceivePointer(pointer)) return false;

            if (pointer.GetPressData().Target != cachedTransform) return false;
            return true;
        }

        /// <inheritdoc />
        public override bool CanPreventGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        public override bool CanBePreventedByGesture(Gesture gesture)
        {
            if (Delegate == null) return false;
            return !Delegate.ShouldRecognizeSimultaneously(this, gesture);
        }

        /// <inheritdoc />
        protected override void pointersPressed(IList<Pointer> pointers)
        {
            base.pointersPressed(pointers);

            if (pointersNumState == PointersNumState.PassedMinThreshold)
            {
                setState(GestureState.Recognized);
                return;
            }
            if (pointersNumState == PointersNumState.PassedMinMaxThreshold)
            {
                setState(GestureState.Failed);
                return;
            }
        }

        /// <inheritdoc />
        protected override void onRecognized()
        {
            base.onRecognized();
            if (pressedInvoker != null) pressedInvoker.InvokeHandleExceptions(this, EventArgs.Empty);
            if (UseSendMessage && SendMessageTarget != null)
                SendMessageTarget.SendMessage(PRESS_MESSAGE, this, SendMessageOptions.DontRequireReceiver);
        }

        #endregion
    }
}