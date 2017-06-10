namespace Ensage.SDK.Samples
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reflection;

    using Ensage.Common.Extensions;
    using Ensage.SDK.Abilities;
    using Ensage.SDK.Abilities.npc_dota_hero_vengefulspirit;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Input;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;
    using Ensage.SDK.TargetSelector;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using SharpDX;

    [ExportPlugin("SamplePredictionSkillshot", StartupMode.Manual)]
    public class SamplePredictionSkillshot : Plugin
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Lazy<IInputManager> input;

        private readonly Lazy<IOrbwalkerManager> orbwalkerManager;

        private readonly Lazy<ITargetSelectorManager> targetManager;

        private readonly Unit owner;

        private PredictionAbility skillshotAbility;

        [ImportingConstructor]
        public SamplePredictionSkillshot(
            [Import] IServiceContext context,
            [Import] Lazy<IOrbwalkerManager> orbManager,
            [Import] Lazy<ITargetSelectorManager> targetManager,
            [Import] Lazy<IInputManager> input)
        {
            this.owner = context.Owner;
            this.input = input;
            this.orbwalkerManager = orbManager;
            this.targetManager = targetManager;
        }

        protected override void OnActivate()
        {
            // this.skillshotAbility = new pudge_meat_hook(this.owner.GetAbilityById(AbilityId.pudge_meat_hook));
            this.skillshotAbility = new vengefulspirit_wave_of_terror(this.owner.GetAbilityById(AbilityId.vengefulspirit_wave_of_terror));
            UpdateManager.Subscribe(this.OnUpdate);
            Drawing.OnDraw += this.Drawing_OnDraw;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            var target = this.targetManager.Value.Active.GetTargets().FirstOrDefault();
            if (target != null)
            {
                var input = this.skillshotAbility.GetPredictionInput(target);
                var output = this.skillshotAbility.GetPredictionOutput(input);

                Vector2 screenPos;
                if (Drawing.WorldToScreen(output.CastPosition, out screenPos))
                {
                    Drawing.DrawCircle(screenPos, 40, 64, Color.Red);
                }
                if (Drawing.WorldToScreen(output.UnitPosition, out screenPos))
                {
                    Drawing.DrawCircle(screenPos, 40, 64, Color.Green);
                }
            }
        }

        protected override void OnDeactivate()
        {
        }

        private void OnUpdate()
        {
            var target = this.targetManager.Value.Active.GetTargets().FirstOrDefault(x => x.Distance2D(this.owner) <= this.skillshotAbility.Range);
            if (target != null)
            {
                this.skillshotAbility.UseAbility(target);
            }
        }
    }
}