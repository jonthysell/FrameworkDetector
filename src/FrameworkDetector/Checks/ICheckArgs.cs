// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector.Checks;


public interface ICheckArgs
{
    string GetDescription();

    void Validate();
}

public class CustomCheckArgs(string description) : ICheckArgs
{
    public string GetDescription() => description;

    public void Validate() { }
}