namespace RepTrackDomain.Enums
{
    /// <summary>
    /// Represents the type of fitness goal
    /// </summary>
    public enum GoalType
    {
        /// <summary>
        /// Goal to achieve a specific weight/rep combination for an exercise
        /// </summary>
        Strength = 0,

        /// <summary>
        /// Goal to achieve a certain total volume per workout for an exercise
        /// </summary>
        Volume = 1,

        /// <summary>
        /// Goal to maintain a certain workout frequency
        /// </summary>
        Frequency = 2,

        /// <summary>
        /// Custom goal type for future extensibility
        /// </summary>
        Custom = 3
    }
}