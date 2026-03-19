using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using FlexAnimation;

namespace FlexAnimation.Editor
{
    public class FlexAnimationNode : Node
    {
        public string Guid;
        public FlexAnimationNodeData Data;
        private Label _runtimeValueLabel;
        private VisualElement _runtimeValueBar;
        
        public FlexAnimationNode(FlexAnimationNodeData data)
        {
            Data = data;
            Guid = data.Guid;
            title = data.Title;
            
            SetPosition(new Rect(data.Position, Vector2.zero));
            
            GenerateUI();
            CreateRuntimeVisuals();
        }

        private void CreateRuntimeVisuals()
        {
            var runtimeContainer = new VisualElement();
            runtimeContainer.style.flexDirection = FlexDirection.Row;
            runtimeContainer.style.alignItems = Align.Center;
            runtimeContainer.style.backgroundColor = new Color(0, 0, 0, 0.3f);
            runtimeContainer.style.paddingLeft = 5;
            runtimeContainer.style.paddingRight = 5;
            runtimeContainer.style.height = 20;

            _runtimeValueBar = new VisualElement();
            _runtimeValueBar.style.width = 0;
            _runtimeValueBar.style.height = 4;
            _runtimeValueBar.style.backgroundColor = new Color(0.2f, 0.8f, 1.0f, 0.8f);
            _runtimeValueBar.style.position = Position.Absolute;
            _runtimeValueBar.style.bottom = 0;
            _runtimeValueBar.style.left = 0;
            
            _runtimeValueLabel = new Label("0.00");
            _runtimeValueLabel.style.fontSize = 10;
            _runtimeValueLabel.style.color = Color.white;

            runtimeContainer.Add(_runtimeValueLabel);
            mainContainer.Add(runtimeContainer);
            mainContainer.Add(_runtimeValueBar);
        }

        public void UpdateRuntimeValue(float value)
        {
            _runtimeValueLabel.text = value.ToString("F2");
            _runtimeValueBar.style.width = Length.Percent(Mathf.Clamp01(value) * 100f);
            
            // Visual feedback: intensity based on value
            _runtimeValueBar.style.backgroundColor = Color.Lerp(new Color(0.2f, 0.5f, 1f, 0.5f), new Color(0.2f, 1f, 0.8f, 1f), value);
        }

        private void GenerateUI()
        {
            switch (Data.NodeType)
            {
                case FlexAnimationNodeType.Clip:
                    titleContainer.style.backgroundColor = new Color(0.12f, 0.45f, 0.7f, 0.8f);
                    AddInputPort("Speed");
                    AddOutputPort("Out");
                    
                    var clipField = new ObjectField("Clip") { objectType = typeof(AnimationClip), value = Data.Clip };
                    clipField.RegisterValueChangedCallback(evt => Data.Clip = evt.newValue as AnimationClip);
                    extensionContainer.Add(clipField);
                    
                    var speedSlider = new Slider("Speed", 0.1f, 5.0f) { value = Data.Speed };
                    speedSlider.RegisterValueChangedCallback(evt => Data.Speed = evt.newValue);
                    extensionContainer.Add(speedSlider);
                    break;
                    
                case FlexAnimationNodeType.Blend:
                    titleContainer.style.backgroundColor = new Color(0.2f, 0.6f, 0.3f, 0.8f);
                    AddInputPort("A");
                    AddInputPort("B");
                    AddInputPort("Factor");
                    AddOutputPort("Out");
                    
                    var blendSlider = new Slider("Factor", 0f, 1.0f) { value = Data.BlendValue };
                    blendSlider.RegisterValueChangedCallback(evt => Data.BlendValue = evt.newValue);
                    extensionContainer.Add(blendSlider);
                    break;
                    
                case FlexAnimationNodeType.Output:
                    titleContainer.style.backgroundColor = new Color(0.8f, 0.4f, 0.1f, 0.8f);
                    AddInputPort("Result");
                    break;

                case FlexAnimationNodeType.Lerp:
                    titleContainer.style.backgroundColor = new Color(0.5f, 0.2f, 0.7f, 0.8f);
                    AddInputPort("A");
                    AddInputPort("B");
                    AddInputPort("T");
                    AddOutputPort("Out");
                    break;

                case FlexAnimationNodeType.FlexPreset:
                    titleContainer.style.backgroundColor = new Color(0.8f, 0.2f, 0.4f, 0.8f);
                    AddInputPort("Speed");
                    AddOutputPort("Out");

                    var presetField = new ObjectField("Preset") { objectType = typeof(FlexAnimationPreset), value = Data.Preset };
                    presetField.RegisterValueChangedCallback(evt => Data.Preset = evt.newValue as FlexAnimationPreset);
                    extensionContainer.Add(presetField);

                    var buttonContainer = new VisualElement();
                    buttonContainer.style.flexDirection = FlexDirection.Row;
                    buttonContainer.style.justifyContent = Justify.SpaceBetween;

                    var createBtn = new Button(() => 
                    {
                        string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Create New Preset", "New Flex Preset", "asset", "Save preset to folder");
                        if (!string.IsNullOrEmpty(path))
                        {
                            if (path.StartsWith(Application.dataPath))
                                path = "Assets" + path.Substring(Application.dataPath.Length);
                                
                            var newPreset = ScriptableObject.CreateInstance<FlexAnimationPreset>();
                            UnityEditor.AssetDatabase.CreateAsset(newPreset, path);
                            UnityEditor.AssetDatabase.SaveAssets();
                            
                            Data.Preset = newPreset;
                            presetField.value = newPreset;
                        }
                    }) { text = "New" };
                    createBtn.style.flexGrow = 1;
                    
                    var editBtn = new Button(() => 
                    {
                        if (Data.Preset != null)
                        {
                            UnityEditor.Selection.activeObject = Data.Preset;
                            UnityEditor.EditorGUIUtility.PingObject(Data.Preset);
                        }
                        else
                        {
                            Debug.LogWarning("No Preset assigned to this node.");
                        }
                    }) { text = "Edit" };
                    editBtn.style.flexGrow = 1;

                    buttonContainer.Add(createBtn);
                    buttonContainer.Add(editBtn);
                    extensionContainer.Add(buttonContainer);

                    var presetSpeedSlider = new Slider("Speed", 0.1f, 5.0f) { value = Data.Speed };
                    presetSpeedSlider.RegisterValueChangedCallback(evt => Data.Speed = evt.newValue);
                    extensionContainer.Add(presetSpeedSlider);
                    break;

                case FlexAnimationNodeType.OrganicNoise:
                    titleContainer.style.backgroundColor = new Color(0.1f, 0.7f, 0.4f, 0.8f);
                    AddInputPort("In");
                    AddOutputPort("Out");

                    var freqSlider = new Slider("Frequency", 0.1f, 20.0f) { value = Data.NoiseFrequency };
                    freqSlider.tooltip = "How fast the noise oscillates.";
                    freqSlider.RegisterValueChangedCallback(evt => Data.NoiseFrequency = evt.newValue);
                    extensionContainer.Add(freqSlider);

                    var ampSlider = new Slider("Amplitude", 0.0f, 2.0f) { value = Data.NoiseAmplitude };
                    ampSlider.tooltip = "The maximum distance of the movement.";
                    ampSlider.RegisterValueChangedCallback(evt => Data.NoiseAmplitude = evt.newValue);
                    extensionContainer.Add(ampSlider);

                    var axisField = new Vector3Field("Axis") { value = Data.NoiseAxis };
                    axisField.tooltip = "The direction of the noise movement.";
                    axisField.RegisterValueChangedCallback(evt => Data.NoiseAxis = evt.newValue);
                    extensionContainer.Add(axisField);
                    break;

                case FlexAnimationNodeType.ShaderProperty:
                    titleContainer.style.backgroundColor = new Color(0.6f, 0.2f, 0.8f, 0.8f);
                    AddInputPort("Value");
                    AddOutputPort("Out");

                    var matField = new ObjectField("Material") { objectType = typeof(Material), value = Data.TargetMaterial };
                    matField.RegisterValueChangedCallback(evt => Data.TargetMaterial = evt.newValue as Material);
                    extensionContainer.Add(matField);

                    var propNameField = new TextField("Prop Name") { value = Data.PropertyName };
                    propNameField.RegisterValueChangedCallback(evt => Data.PropertyName = evt.newValue);
                    extensionContainer.Add(propNameField);

                    var propTypeField = new EnumField("Type", Data.PropertyType);
                    propTypeField.RegisterValueChangedCallback(evt => 
                    {
                        Data.PropertyType = (FlexShaderPropertyType)evt.newValue;
                        GenerateUI(); // Refresh UI to show correct value field
                    });
                    extensionContainer.Add(propTypeField);

                    if (Data.PropertyType == FlexShaderPropertyType.Float)
                    {
                        var fValField = new FloatField("Value") { value = Data.FloatValue };
                        fValField.RegisterValueChangedCallback(evt => Data.FloatValue = evt.newValue);
                        extensionContainer.Add(fValField);
                    }
                    else
                    {
                        var cValField = new ColorField("Value") { value = Data.ColorValue };
                        cValField.RegisterValueChangedCallback(evt => Data.ColorValue = evt.newValue);
                        extensionContainer.Add(cValField);
                    }
                    break;

                case FlexAnimationNodeType.ShaderEffect:
                    titleContainer.style.backgroundColor = new Color(0.9f, 0.2f, 0.6f, 0.8f);
                    AddInputPort("In");
                    AddOutputPort("Out");

                    var effectMatField = new ObjectField("Material") { objectType = typeof(Material), value = Data.TargetMaterial };
                    effectMatField.RegisterValueChangedCallback(evt => Data.TargetMaterial = evt.newValue as Material);
                    extensionContainer.Add(effectMatField);

                    var effectPropNameField = new TextField("Prop Name") { value = Data.PropertyName };
                    effectPropNameField.RegisterValueChangedCallback(evt => Data.PropertyName = evt.newValue);
                    extensionContainer.Add(effectPropNameField);

                    var effectTypeField = new EnumField("Effect Type", Data.EffectType);
                    effectTypeField.RegisterValueChangedCallback(evt => Data.EffectType = (FlexShaderEffectType)evt.newValue);
                    extensionContainer.Add(effectTypeField);

                    var effectFreqSlider = new Slider("Frequency", 0.1f, 10.0f) { value = Data.EffectFrequency };
                    effectFreqSlider.RegisterValueChangedCallback(evt => Data.EffectFrequency = evt.newValue);
                    extensionContainer.Add(effectFreqSlider);

                    var effectIntenSlider = new Slider("Intensity", 0.0f, 5.0f) { value = Data.EffectIntensity };
                    effectIntenSlider.RegisterValueChangedCallback(evt => Data.EffectIntensity = evt.newValue);
                    extensionContainer.Add(effectIntenSlider);

                    var minMaxContainer = new VisualElement();
                    minMaxContainer.style.flexDirection = FlexDirection.Row;
                    
                    var minField = new FloatField("Min") { value = Data.EffectMin };
                    minField.style.flexGrow = 1;
                    minField.RegisterValueChangedCallback(evt => Data.EffectMin = evt.newValue);
                    
                    var maxField = new FloatField("Max") { value = Data.EffectMax };
                    maxField.style.flexGrow = 1;
                    maxField.RegisterValueChangedCallback(evt => Data.EffectMax = evt.newValue);
                    
                    minMaxContainer.Add(minField);
                    minMaxContainer.Add(maxField);
                    extensionContainer.Add(minMaxContainer);

                    var effectColorField = new ColorField("Base Color") { value = Data.ColorValue };
                    effectColorField.RegisterValueChangedCallback(evt => Data.ColorValue = evt.newValue);
                    extensionContainer.Add(effectColorField);
                    break;

                case FlexAnimationNodeType.Spring:
                    titleContainer.style.backgroundColor = new Color(0.1f, 0.6f, 0.7f, 0.8f);
                    AddInputPort("In");
                    AddOutputPort("Out");

                    var stiffSlider = new Slider("Stiffness", 1.0f, 500.0f) { value = Data.Stiffness };
                    stiffSlider.RegisterValueChangedCallback(evt => Data.Stiffness = evt.newValue);
                    extensionContainer.Add(stiffSlider);

                    var dampSlider = new Slider("Damping", 0.1f, 50.0f) { value = Data.Damping };
                    dampSlider.RegisterValueChangedCallback(evt => Data.Damping = evt.newValue);
                    extensionContainer.Add(dampSlider);
                    break;

                case FlexAnimationNodeType.PointerInput:
                    titleContainer.style.backgroundColor = new Color(0.8f, 0.5f, 0.1f, 0.8f);
                    AddOutputPort("Value");

                    var pModeField = new EnumField("Mode", Data.PointerMode);
                    pModeField.RegisterValueChangedCallback(evt => Data.PointerMode = (FlexPointerMode)evt.newValue);
                    extensionContainer.Add(pModeField);

                    var rangeField = new FloatField("Range") { value = Data.InteractionRange };
                    rangeField.RegisterValueChangedCallback(evt => Data.InteractionRange = evt.newValue);
                    extensionContainer.Add(rangeField);
                    break;

                case FlexAnimationNodeType.AudioReactor:
                    titleContainer.style.backgroundColor = new Color(1.0f, 0.8f, 0.2f, 0.8f);
                    AddOutputPort("Value");

                    var bandField = new EnumField("Band", Data.AudioBand);
                    bandField.RegisterValueChangedCallback(evt => Data.AudioBand = (FlexAudioBand)evt.newValue);
                    extensionContainer.Add(bandField);

                    var sensitivitySlider = new Slider("Sensitivity", 0.1f, 10.0f) { value = Data.AudioSensitivity };
                    sensitivitySlider.RegisterValueChangedCallback(evt => Data.AudioSensitivity = evt.newValue);
                    extensionContainer.Add(sensitivitySlider);

                    var thresholdSlider = new Slider("Threshold", 0.0f, 0.5f) { value = Data.AudioThreshold };
                    thresholdSlider.RegisterValueChangedCallback(evt => Data.AudioThreshold = evt.newValue);
                    extensionContainer.Add(thresholdSlider);
                    break;

                case FlexAnimationNodeType.TimeRewind:
                    titleContainer.style.backgroundColor = new Color(0.1f, 0.2f, 0.4f, 0.8f);
                    AddInputPort("Rewind Control");
                    AddOutputPort("Out");

                    var bufferSlider = new Slider("Buffer (Sec)", 0.5f, 10.0f) { value = Data.BufferSeconds };
                    bufferSlider.RegisterValueChangedCallback(evt => Data.BufferSeconds = evt.newValue);
                    extensionContainer.Add(bufferSlider);
                    break;

                case FlexAnimationNodeType.ParticleControl:
                    titleContainer.style.backgroundColor = new Color(1.0f, 0.4f, 0.0f, 0.8f);
                    AddInputPort("Value");

                    var psField = new ObjectField("Particle System") { objectType = typeof(ParticleSystem), value = Data.TargetParticle };
                    psField.RegisterValueChangedCallback(evt => Data.TargetParticle = evt.newValue as ParticleSystem);
                    extensionContainer.Add(psField);

                    var pFieldEnum = new EnumField("Control Field", Data.ParticleField);
                    pFieldEnum.RegisterValueChangedCallback(evt => Data.ParticleField = (FlexParticleControlField)evt.newValue);
                    extensionContainer.Add(pFieldEnum);

                    var pMinMaxContainer = new VisualElement();
                    pMinMaxContainer.style.flexDirection = FlexDirection.Row;
                    var pMinField = new FloatField("Min") { value = Data.ParticleMin };
                    pMinField.style.flexGrow = 1;
                    pMinField.RegisterValueChangedCallback(evt => Data.ParticleMin = evt.newValue);
                    var pMaxField = new FloatField("Max") { value = Data.ParticleMax };
                    pMaxField.style.flexGrow = 1;
                    pMaxField.RegisterValueChangedCallback(evt => Data.ParticleMax = evt.newValue);
                    pMinMaxContainer.Add(pMinField);
                    pMinMaxContainer.Add(pMaxField);
                    extensionContainer.Add(pMinMaxContainer);

                    var pColorField = new ColorField("Target Color") { value = Data.ParticleColor };
                    pColorField.RegisterValueChangedCallback(evt => Data.ParticleColor = evt.newValue);
                    extensionContainer.Add(pColorField);
                    break;

                case FlexAnimationNodeType.Wait:
                    titleContainer.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                    AddInputPort("In");
                    AddOutputPort("Out");

                    var waitSlider = new Slider("Duration", 0.0f, 10.0f) { value = Data.Duration };
                    waitSlider.RegisterValueChangedCallback(evt => Data.Duration = evt.newValue);
                    extensionContainer.Add(waitSlider);
                    break;

                case FlexAnimationNodeType.Compare:
                    titleContainer.style.backgroundColor = new Color(0.4f, 0.1f, 0.5f, 0.8f);
                    AddInputPort("In");
                    AddOutputPort("Out");

                    var compareTypeField = new EnumField("Compare", Data.CompareType);
                    compareTypeField.RegisterValueChangedCallback(evt => Data.CompareType = (FlexCompareType)evt.newValue);
                    extensionContainer.Add(compareTypeField);

                    var thresholdField = new FloatField("Threshold") { value = Data.Threshold };
                    thresholdField.RegisterValueChangedCallback(evt => Data.Threshold = evt.newValue);
                    extensionContainer.Add(thresholdField);
                    break;

                case FlexAnimationNodeType.Sequence:
                    titleContainer.style.backgroundColor = new Color(0.2f, 0.1f, 0.4f, 0.8f);
                    AddInputPort("Step 1");
                    AddInputPort("Step 2");
                    AddInputPort("Step 3");
                    AddInputPort("Step 4");
                    AddOutputPort("Out");

                    var stepSlider = new Slider("Step Time", 0.1f, 5.0f) { value = Data.Duration };
                    stepSlider.RegisterValueChangedCallback(evt => Data.Duration = evt.newValue);
                    extensionContainer.Add(stepSlider);
                    break;

                case FlexAnimationNodeType.GlobalSet:
                    titleContainer.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                    AddInputPort("Value");

                    var setKeyField = new TextField("Variable Name") { value = Data.GlobalKey };
                    setKeyField.RegisterValueChangedCallback(evt => Data.GlobalKey = evt.newValue);
                    extensionContainer.Add(setKeyField);
                    break;

                case FlexAnimationNodeType.GlobalGet:
                    titleContainer.style.backgroundColor = new Color(0.2f, 0.6f, 1.0f, 0.8f);
                    AddOutputPort("Value");

                    var getKeyField = new TextField("Variable Name") { value = Data.GlobalKey };
                    getKeyField.RegisterValueChangedCallback(evt => Data.GlobalKey = evt.newValue);
                    extensionContainer.Add(getKeyField);
                    break;

                case FlexAnimationNodeType.Math:
                    titleContainer.style.backgroundColor = new Color(0.7f, 0.1f, 0.1f, 0.8f);
                    AddInputPort("A");
                    AddInputPort("B");
                    AddOutputPort("Out");

                    var mathOpField = new EnumField("Operation", Data.MathOp);
                    mathOpField.RegisterValueChangedCallback(evt => Data.MathOp = (FlexMathOp)evt.newValue);
                    extensionContainer.Add(mathOpField);
                    break;

                case FlexAnimationNodeType.Custom:
                    titleContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    AddInputPort("In");

                    var compField = new ObjectField("Component") { objectType = typeof(Component), value = Data.CustomComponent };
                    compField.RegisterValueChangedCallback(evt => Data.CustomComponent = evt.newValue as Component);
                    extensionContainer.Add(compField);

                    var methodField = new TextField("Method Name") { value = Data.CustomMethod };
                    methodField.RegisterValueChangedCallback(evt => Data.CustomMethod = evt.newValue);
                    extensionContainer.Add(methodField);
                    break;
            }
            
            RefreshExpandedState();
            RefreshPorts();
        }

        public void AddInputPort(string portName, Port.Capacity capacity = Port.Capacity.Single)
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, typeof(float));
            inputPort.portName = portName;
            inputContainer.Add(inputPort);
        }
        
        public void AddOutputPort(string portName, Port.Capacity capacity = Port.Capacity.Single)
        {
            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, typeof(float));
            outputPort.portName = portName;
            outputContainer.Add(outputPort);
        }
    }
}
