// (c) Microsoft. All rights reserved
//-----------------------
//
// Generate RANDOMly populated items...
//
//-----------------------

function setCommonData(item) {
    item.itemData.common.source = "HVTEST - Win8 Javascript Test";
    item.itemData.common.note = "Randomly Generated";
}

function makeAddress() {
    
    var address = new HealthVault.Types.Address();
    address.street = new Array(
        "1234",
        "Princess Street"
    );
    address.city = "Edinburgh";
    address.postalCode = "ABCDEF";
    address.country = "Scotland";

    return address;
}

function makeEmail() {
}

function makeContact() {
    var contact = new HealthVault.Types.Contact();
    var address = makeAddress();

    contact.address = new Array(
        address
    );

    contact.email = new Array(
        new HealthVault.Types.Email("foo@bar.xyz")
    );
    contact.phone = new Array(
        new HealthVault.Types.Phone("555-555-5555")
    );

    return contact;
}

function makePerson() {
    var person = new HealthVault.Types.Person();
    person.name = new HealthVault.Types.Name("Toby", "Riley", "McDuff");
    person.organization = "Justice League of Medicine";
    person.training = "MD, Phd, FRCS, PQRS";
    person.contact = makeContact();

    return person;
}

function makeAllergy() {
    var allergen = Array.randomItemFrom("Pollen", "Peanuts", "Penicillin", "Animal Dander", "Dust");

    var allergy = new HealthVault.ItemTypes.Allergy(allergen);
    allergy.firstObserved = new HealthVault.Types.ApproxDateTime();
    allergy.firstObserved.description = Array.randomItemFrom("In my teens", "As a child", "As an adult");
    allergy.reaction = new HealthVault.Types.CodableValue(Array.randomItemFrom("anaphylactic shock", "sneezing", "Nausea"));

    return allergy;
}

function makeCondition() {
    var conditionName = Array.randomItemFrom("Migraine", "Pancreatitis", "Mild Depression", "Ulcer", "Asthma");

    var condition = new HealthVault.ItemTypes.Condition(conditionName);
    condition.status = new HealthVault.Types.CodableValue(Array.randomItemFrom("chronic", "acute"));
    if (Math.random() > 0.5) {
        condition.onsetDate = new HealthVault.Types.ApproxDateTime(Date.random());
    }
    else {
        condition.onsetDate = new HealthVault.Types.ApproxDateTime();
        condition.onsetDate.description = Array.randomItemFrom("As a teenager", "As a child");
    }

    setCommonData(condition);

    return condition;
}

function makeHeight() {

    var length = new HealthVault.Types.LengthMeasurement();
    length.inInches = Math.randomIntInRange(60, 70);

    var height = new HealthVault.ItemTypes.Height(HealthVault.Types.StructuredDateTime.now(), length);

    setCommonData(height);

    return height;
}

function makeMedication() {
    
    var drugName = Array.randomItemFrom("Lipitor", "Ibuprofen", "Celebrex", "Prozac", "Claritin", "Viagra", "Omega 3 Supplement", "Multivitamin");

    var med = new HealthVault.ItemTypes.Medication(drugName);
    med.dose = new HealthVault.Types.ApproxMeasurement(Math.randomIntInRange(1, 4), "Tablets", "Tablets", "medication-strength-unit");
    med.strength = new HealthVault.Types.ApproxMeasurement(Math.randomIntInRange(100, 1000), "Milligrams", "mg", "medication-strength-unit");
    med.frequency = new HealthVault.Types.ApproxMeasurement(Array.randomItemFrom("Once a day", "Twice a day", "As needed"));
    med.startDate = new HealthVault.Types.ApproxDateTime(Date.random(365));

    setCommonData(med);

    return med;    
}

function makeProcedure() {

    var name = Array.randomItemFrom("eye surgery", "root canal", "colonoscopy", "knee surgery", "tooth cleaning");

    var proc = new HealthVault.ItemTypes.Procedure(name);
    proc.when = new HealthVault.Types.ApproxDateTime(Date.random());

    proc.primaryProvider = makePerson();

    return proc;
}

function makeImmunization() {
    var name = Array.randomItemFrom("hepatitis A vaccine", "Polio", "Smallpox Shot", "Measles", "Anthrax Vaccine");
    var codableName = new HealthVault.Types.CodableValue(name, "vaccines-foo", "xyzqr");

    var immunization = new HealthVault.ItemTypes.Immunization();
    immunization.name = codableName;
    immunization.administrationDate = new HealthVault.Types.ApproxDateTime(Date.random());
    immunization.manufacturer = new HealthVault.Types.CodableValue("Generic Manufacturer");
    immunization.lot = Math.randomIntInRange(100, 500);
    immunization.sequence = Math.randomInRange(1, 3);
    immunization.anatomicSurface = new HealthVault.Types.CodableValue("Arm");

    return immunization;
}


function makeWeight() {

    var weightValue = new HealthVault.Types.WeightMeasurement();
    weightValue.inPounds = Math.roundToPrecision(Math.randomInRange(120, 150), 1);

    var weight = new HealthVault.ItemTypes.Weight(HealthVault.Types.StructuredDateTime.now(), weightValue);

    setCommonData(weight);

    return weight;
}

function makeBloodPressureRandom() {

    var bloodPressure = new HealthVault.ItemTypes.BloodPressure();
    bloodPressure.when = HealthVault.Types.StructuredDateTime.now();

    var systolic = Math.randomIntInRange(150, 120);
    bloodPressure.systolic = new HealthVault.Types.NonNegativeInt(systolic);
    bloodPressure.diastolicValue = Math.randomIntInRange(70, systolic);
    bloodPressure.pulseValue = Math.randomIntInRange(60, 100);
    bloodPressure.irregularHeartbeat = new HealthVault.Types.BooleanValue(false);

    setCommonData(bloodPressure);

    return bloodPressure;
}

function makeBloodPressure() {

    var bloodPressure = new HealthVault.ItemTypes.BloodPressure();
    bloodPressure.when = HealthVault.Types.StructuredDateTime.now();
    bloodPressure.systolic = new HealthVault.Types.NonNegativeInt(120);
    bloodPressure.diastolicValue= 80;
    bloodPressure.pulseValue = 58;
    bloodPressure.irregularHeartbeat = new HealthVault.Types.BooleanValue(false);

    setCommonData(bloodPressure);
    
    return bloodPressure;
}

function makeCholesterol() {

    var cholesterol = new HealthVault.ItemTypes.Cholesterol();
    cholesterol.when = HealthVault.Types.StructuredDateTime.now();
    cholesterol.ldlValue = new HealthVault.Types.ConcentrationValue(100);
    cholesterol.hdlValue = new HealthVault.Types.ConcentrationValue(200);
    cholesterol.total = new HealthVault.Types.ConcentrationValue(300);
    cholesterol.triglycerides = new HealthVault.Types.ConcentrationValue(75);

    setCommonData(cholesterol);

    return cholesterol;
}

function makeExercise() {

    var exercise = new HealthVault.ItemTypes.Exercise();
    exercise.when = HealthVault.Types.ApproxDateTime.now();
    var distance = new HealthVault.Types.LengthMeasurement();
    distance.inMiles = 5.5;
    exercise.distance = distance;

    exercise.activity = new HealthVault.Types.CodableValue("Biking");
    exercise.details.append(
        new HealthVault.Types.StructuredNameValue(
            new HealthVault.Types.CodedValue("myvocab", "mycode"),
        new HealthVault.Types.StructuredMeasurement("750", "calories")));

    exercise.details.append(
        new HealthVault.Types.StructuredNameValue(
            new HealthVault.Types.CodedValue("myvocab", "avg-cadence"),
        new HealthVault.Types.StructuredMeasurement("85", "rpm")));

    var segment = new HealthVault.Types.ExerciseSegment();
    segment.activity = new HealthVault.Types.CodableValue("Biking");
    segment.title = "Finn Hill";
    segment.distance = distance;
    segment.duration = new HealthVault.Types.PositiveDouble("78.6");
    segment.offset = new HealthVault.Types.NonNegativeDouble("10.2");
    segment.details = exercise.details;

    exercise.segments.append(segment);

    setCommonData(exercise);

    return exercise;
}

function makeEmergencyContact() {
    var emergencyContact = new HealthVault.ItemTypes.Contact();

    var address = makeAddress();
    var contact = makeContact();
    var contactType = new HealthVault.Types.CodableValue("Emergency Contact", new HealthVault.Types.CodedValue("person-types", "1"));

    contact.address = [address];

    emergencyContact.name = new HealthVault.Types.Name("test", "example" + Math.randomIntInRange(1, 10000));
    emergencyContact.contactInformation = contact;
    emergencyContact.contactType = contactType;

    setCommonData(emergencyContact);

    return emergencyContact;
}

function makeInsurance() {
    var insurance = new HealthVault.ItemTypes.Insurance();
    var contact = makeContact();

    insurance.planName = "Premera Blue Cross";
    insurance.coverageType = new HealthVault.Types.CodableValue("medical", "1", "coverage-types");
    insurance.carrierId = "BCBS430";
    insurance.groupNumber = "111111";
    insurance.planCode = "MSJ";
    insurance.subscriberId = "1234";
    insurance.personCode = "01";
    insurance.subscriberName = "Myself";
    insurance.subscriberDob = new HealthVault.Types.StructuredDateTime(new Date(1999, 11, 17));
    insurance.isPrimary = new HealthVault.Types.BooleanValue(true);
    insurance.expirationDate = new HealthVault.Types.StructuredDateTime(new Date(2013, 11, 17));
    insurance.contact = contact;

    return insurance;
}

function makeHealthGoal() {
    var healthGoal = new HealthVault.ItemTypes.HealthGoal();

    healthGoal.name = new HealthVault.Types.CodableValue("weight-range", "goal-type", "weight");
    healthGoal.description = "description";
    healthGoal.startDate = new HealthVault.Types.ApproxDateTime(new Date(2012, 2, 1));
    healthGoal.endDate = new HealthVault.Types.ApproxDateTime(new Date(2013, 2, 1));
    healthGoal.associatedTypeInfo = makeGoalAssociatedTypeInfo();
    healthGoal.targetRange = makeWeightRange(130, 150);
    healthGoal.goalAdditionalRanges = makeWeightGoalAdditionalRanges();
    
    healthGoal.recurrence = new HealthVault.Types.GoalRecurrence();
    healthGoal.recurrence.interval = new HealthVault.Types.CodableValue("day");
    healthGoal.recurrence.timesInInterval = "1";
    
    return healthGoal;
}

function makeWeightGoalAdditionalRanges() {
    var longRange = new HealthVault.Types.GoalRange();
    longRange.name = new HealthVault.Types.CodableValue("range");
    
    longRange.minimum = new HealthVault.Types.GeneralMeasurement();
    longRange.minimum.display = "120";
    longRange.minimum.structured = [
        new HealthVault.Types.StructuredMeasurement(120, "kg"),
        new HealthVault.Types.StructuredMeasurement(121, "kg"),
        new HealthVault.Types.StructuredMeasurement(122, "kg")];
    
    longRange.maximum = new HealthVault.Types.GeneralMeasurement();
    longRange.maximum.display = "130";
    longRange.maximum.structured = [
        new HealthVault.Types.StructuredMeasurement(130, "kg"),
        new HealthVault.Types.StructuredMeasurement(131, "kg"),
        new HealthVault.Types.StructuredMeasurement(132, "kg")];

    return [
        longRange,
        makeWeightRange(null, 100 * Math.random()),
        makeWeightRange(100 * Math.random(), null),
        makeWeightRange(10, 10)
    ];
}

function makeWeightRange(min, max) {
    var unitsCode = new HealthVault.Types.CodedValue("weight-units", "lb");
    unitsCode.vocabFamily = "wc";

    var range = new HealthVault.Types.GoalRange();
    range.name = new HealthVault.Types.CodableValue("range");
    range.description = "range description";

    if (min) {
        range.minimum = new HealthVault.Types.GeneralMeasurement();
        range.minimum.display = min;
        range.minimum.structured = [new HealthVault.Types.StructuredMeasurement(min, "lb")];
        range.minimum.structured[0].units.codes.append(unitsCode);
    }

    if (max) {
        range.maximum = new HealthVault.Types.GeneralMeasurement();
        range.maximum.display = max;
        range.maximum.structured = [new HealthVault.Types.StructuredMeasurement(max, "lb")];
        range.maximum.structured[0].units.codes.append(unitsCode);
    }

    return range;
}

function makeGoalAssociatedTypeInfo() {
    var typeInfo = new HealthVault.Types.GoalAssociatedTypeInfo();
    typeInfo.thingTypeVersionId = "C0DEBEEF-C0DE-BEEF-C0DE-BEEFC0DEBEEF";
    typeInfo.thingTypeDisplayXpath = "display-xpath";
    typeInfo.thingTypeValueXpath = "value-xpath";
    return typeInfo;
}

function makeBloodGlucose() {
    var wholeBlood = new HealthVault.Types.CodedValue();
    wholeBlood.code = "wb";
    wholeBlood.vocabFamily = "wc";
    wholeBlood.vocabName = "glucose-measurement-type";
    wholeBlood.vocabVersion = "1";

    var fasting = new HealthVault.Types.CodedValue();
    fasting.code = "fasting";
    fasting.vocabFamily = "wc";
    fasting.vocabName = "glucose-measurement-context";
    fasting.vocabVersion = "1";

    var glucose = new HealthVault.ItemTypes.BloodGlucose();
    glucose.when = HealthVault.Types.StructuredDateTime.now();
    glucose.value = new HealthVault.Types.BloodGlucoseMeasurement(3 + (4 * Math.random()));
    glucose.measurementType = new HealthVault.Types.CodableValue("Whole Blood", wholeBlood);
    glucose.outsideOperatingTemperature = new HealthVault.Types.BooleanValue(false);
    glucose.isControlTest = new HealthVault.Types.BooleanValue(false);
    glucose.normalcy = new HealthVault.Types.OneToFive(3);
    glucose.measurementContext = new HealthVault.Types.CodableValue("Fasting", fasting);
    return glucose;
}