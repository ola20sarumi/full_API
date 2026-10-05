namespace full.API.Services;

public sealed class DuplicateEmailException(string email)
    : Exception($"An employee with email '{email}' already exists.");
