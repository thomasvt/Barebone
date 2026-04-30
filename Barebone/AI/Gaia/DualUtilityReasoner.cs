namespace Barebone.AI.Gaia
{
    public class DualUtilityReasoner : IGaiaReasoner
    {
        private readonly List<GaiaOption> _candidates = new();
        public readonly GaiaOption[] Options;

        public DualUtilityReasoner(GaiaOption[] options)
        {
            if (options.Length == 0) throw new Exception("Reasoner must have at least 1 option.");
            Options = options;
        }

        private GaiaOption? _currentOption = null;
        public void Update()
        {
            if (_currentOption == null || _currentOption.Action.Interruptable || _currentOption.Action.IsDone)
            {
                var option = ChooseBestOption();
                if (option != _currentOption || _currentOption is { Action.IsDone: true })
                {
                    SwitchTo(option);
                }
            }

            _currentOption?.Action.Update();
        }

        private void SwitchTo(in GaiaOption? option)
        {
            if (_currentOption != null)
            {
                _currentOption.Action.OnLeave();
                if (Debug) Console.WriteLine("LEFT " + _currentOption.Name);
            }

            _currentOption = option;

            if (_currentOption != null)
            {
                if (Debug) Console.WriteLine("ENTER " + _currentOption.Name);
                _currentOption.Action.OnEnter();
            }
        }

        private GaiaOption? ChooseBestOption()
        {
            CollectCandidatesOfHighestRankOnly();

            _candidates.Sort((a, b) => b.Weight.CompareTo(a.Weight)); // sort by descending score.

            return _candidates.FirstOrDefault(); // todo remove all less than 90% of winner's score and pick among remaining candidates by weighted random.
        }

        private void CollectCandidatesOfHighestRankOnly()
        {
            _candidates.Clear();
            var highestRank = int.MinValue;
            var highestScore = float.MinValue;
            foreach (var option in Options)
            {
                if (option.Rank >= highestRank)
                {
                    option.Calculate();
                    if (option.Weight > 0)
                    {
                        if (option.Rank > highestRank)
                        {
                            // new maxrank found!
                            // all previous candidates' rank is now too low, so restart candidate tracking with the current option as the first candidate.
                            _candidates.Clear();
                            _candidates.Add(option);
                            highestScore = option.Weight;
                            highestRank = option.Rank;
                        }
                        else
                        {
                            _candidates.Add(option);
                            highestScore = MathF.Max(highestScore, option.Weight);
                        }
                    }
                }
            }
        }

        public bool Debug { get; set; }
    }
}
