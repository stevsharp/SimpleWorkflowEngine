using SimpleWorkflowEngine.CustomerFeature.Steps;

namespace SimpleWorkflowEngine.CustomerFeature.Factory;

/// <summary>
/// Builds the customer-onboarding <see cref="WorkflowEngine{TContext}"/>.
/// Steps are resolved from DI and composed into the graph below:
/// <code>
/// CheckExisting
///   ├─ AlreadyCompleted -> SkipOnboarding -> Finish
///   ├─ IsVip            -> VipWelcome    -> ProvisionAccount -> Finish
///   └─ otherwise        -> Welcome       -> ProvisionAccount -> Finish
/// </code>
/// Route order matters: <c>AlreadyCompleted</c> is evaluated before <c>IsVip</c>, so a returning VIP
/// short-circuits to <c>SkipOnboarding</c>. An empty <c>CustomerId</c> fails fast at <c>CheckExisting</c>.
/// </summary>
/// <param name="checkExisting">Entry step; validates <c>CustomerId</c> and logs the incoming flags.</param>
/// <param name="welcome">Standard greeting for new, non-VIP customers.</param>
/// <param name="vipWelcome">Greeting for VIP-tier customers.</param>
/// <param name="provision">Account provisioning used by both the standard and VIP paths.</param>
/// <param name="skipOnboarding">Short-circuit for customers whose onboarding is already complete.</param>
/// <param name="finish">Terminal step for every path.</param>
public sealed class CustomerFeatureWorkflowFactory(
    CheckExistingStep checkExisting,
    WelcomeStep welcome,
    VipWelcomeStep vipWelcome,
    ProvisionAccountStep provision,
    SkipOnboardingStep skipOnboarding,
    FinishStep finish)
{
    private readonly CheckExistingStep _checkExisting = checkExisting;
    private readonly WelcomeStep _welcome = welcome;
    private readonly VipWelcomeStep _vipWelcome = vipWelcome;
    private readonly ProvisionAccountStep _provision = provision;
    private readonly SkipOnboardingStep _skipOnboarding = skipOnboarding;
    private readonly FinishStep _finish = finish;

    /// <summary>
    /// Composes a new <see cref="WorkflowEngine{CustomerFeatureContext}"/> wired with the customer steps
    /// and the branching routes. Call once per host; the returned engine is reusable.
    /// </summary>
    public WorkflowEngine<CustomerFeatureContext> Build() =>
        new WorkflowEngine<CustomerFeatureContext>()
            .AddStep(_checkExisting)
        
                .AddRoute(_checkExisting.Name,
                    when: ctx => ctx.AlreadyCompleted,
                    to: _skipOnboarding.Name)

                .AddRoute(_checkExisting.Name,
                    when: ctx => ctx.IsVip,
                    to: _vipWelcome.Name)

                .AddRoute(_checkExisting.Name,
                    when: ctx => !ctx.AlreadyCompleted && !ctx.IsVip,
                    to: _welcome.Name)

            .AddStep(_welcome, nextStep: _provision.Name)
            .AddStep(_vipWelcome, nextStep: _provision.Name)
            .AddStep(_provision, nextStep: _finish.Name)
            .AddStep(_skipOnboarding, nextStep: _finish.Name)
            .AddStep(_finish);
}
