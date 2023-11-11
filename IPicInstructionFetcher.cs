namespace picdasm
{
    interface IPicInstructionFetcher
    {
        void FetchInstruciton(out byte hi, out byte lo);
    }
}
