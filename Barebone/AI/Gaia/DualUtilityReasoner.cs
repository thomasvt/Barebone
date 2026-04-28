namespace Barebone.AI.Gaia
{
    public class DualUtilityReasoner : IGaiaReasoner
    {
        private readonly List<GaiaOption> _candidates = new();
        private readonly GaiaOption[] _options;

        public DualUtilityReasoner(GaiaOption[] options)
        {
            if (options.Length == 0) throw new Exception("Reasoner must have at least 1 option.");
            _options = options;
        }

        private GaiaOption? _currentOption = null;
        public void Update()
        {
            if (_currentOption == null || _currentOption.Action.IsDone)
            {
                var option = ChooseBestOption();
                if (option != _currentOption)
                {
                    if (Debug && _currentOption != null)
                        Console.WriteLine("LEAVE " + _currentOption.Name);
                    _currentOption?.Action.OnLeave();

                    _currentOption = option;

                    option?.Action.OnEnter();
                    if (Debug && option != null)
                        Console.WriteLine("ENTER " + option.Name);
                }
            }

            _currentOption?.Action.Update();
        }

        private GaiaOption? ChooseBestOption()
        {
            CollectCandidatesOfHighestRankOnly();
            
            _candidates.Sort((a, b) => b.Score.CompareTo(a.Score)); // sort by descending score.

            return _candidates.FirstOrDefault(); // todo remove all less than 90% of winner's score and pick among remaining candidates by weighted random.
        }

        private void CollectCandidatesOfHighestRankOnly()
        {
            _candidates.Clear();
            var highestRank = int.MinValue;
            var highestScore = float.MinValue;
            foreach (var option in _options)
            {
                if (option.Rank >= highestRank)
                {
                    option.Calculate();
                    if (option.Score > 0)
                    {
                        if (option.Rank > highestRank)
                        {
                            // new maxrank found!
                            // all previous candidates' rank is now too low, so restart candidate tracking with the current option as the first candidate.
                            _candidates.Clear(); 
                            _candidates.Add(option);
                            highestScore = option.Score;
                            highestRank = option.Rank;
                        }
                        else
                        {
                            _candidates.Add(option);
                            highestScore = MathF.Max(highestScore, option.Score);
                        }
                    }
                }
            }
        }

        public bool Debug { get; set; }
    }
}
