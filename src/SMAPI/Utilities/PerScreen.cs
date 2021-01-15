using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Utilities
{
    /// <summary>Manages a separate value for each player in split-screen mode. This can safely be used in non-split-screen mode too, it'll just have a single state in that case.</summary>
    /// <typeparam name="T">The state class.</typeparam>
    public class PerScreen<T>
    {
        /*********
        ** Fields
        *********/
        /// <summary>Create the initial value for a player.</summary>
        private readonly Func<T> CreateNewState;

        /// <summary>The tracked values for each player.</summary>
        private readonly IDictionary<int, T> States = new Dictionary<int, T>();

        /// <summary>The last <see cref="Context.LastRemovedScreenId"/> value for which this instance was updated.</summary>
        private int LastRemovedScreenId;


        /*********
        ** Accessors
        *********/
        /// <summary>The value for the current player.</summary>
        /// <remarks>The value is initialized the first time it's requested for that player, unless it's set manually first.</remarks>
        public T Value
        {
            get => this.GetValueForScreen(Context.ScreenId);
            set => this.SetValueForScreen(Context.ScreenId, value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public PerScreen()
            : this(null) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="createNewState">Create the initial state for a player screen.</param>
        public PerScreen(Func<T> createNewState)
        {
            this.CreateNewState = createNewState ?? (() => default);
        }

        /// <summary>Get the value for a given screen ID, creating it if needed.</summary>
        /// <param name="screenId">The screen ID to check.</param>
        public T GetValueForScreen(int screenId)
        {
            this.RemoveDeadScreens();
            return this.States.TryGetValue(screenId, out T state)
                ? state
                : this.States[screenId] = this.CreateNewState();
        }

        /// <summary>Set the value for a given screen ID.</summary>
        /// <param name="screenId">The screen ID whose value set.</param>
        /// <param name="value">The value to set.</param>
        public void SetValueForScreen(int screenId, T value)
        {
            this.RemoveDeadScreens();
            this.States[screenId] = value;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Remove screens which are no longer active.</summary>
        private void RemoveDeadScreens()
        {
            if (this.LastRemovedScreenId == Context.LastRemovedScreenId)
                return;
            this.LastRemovedScreenId = Context.LastRemovedScreenId;

            foreach (var pair in this.States.ToArray())
            {
                if (!Context.HasScreenId(pair.Key))
                    this.States.Remove(pair.Key);
            }
        }
    }
}
