import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.StringReader;
import java.io.StringWriter;
import java.security.Key;
import java.security.KeyStore;
import java.security.KeyStoreException;
import java.security.NoSuchAlgorithmException;
import java.security.PublicKey;
import java.security.UnrecoverableEntryException;
import java.security.cert.Certificate;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;

import javax.xml.XMLConstants;
import javax.xml.crypto.AlgorithmMethod;
import javax.xml.crypto.KeySelector;
import javax.xml.crypto.KeySelectorException;
import javax.xml.crypto.KeySelectorResult;
import javax.xml.crypto.XMLCryptoContext;
import javax.xml.crypto.XMLStructure;
import javax.xml.crypto.dom.DOMStructure;
import javax.xml.crypto.dsig.CanonicalizationMethod;
import javax.xml.crypto.dsig.DigestMethod;
import javax.xml.crypto.dsig.Reference;
import javax.xml.crypto.dsig.SignatureMethod;
import javax.xml.crypto.dsig.SignedInfo;
import javax.xml.crypto.dsig.Transform;
import javax.xml.crypto.dsig.XMLSignature;
import javax.xml.crypto.dsig.XMLSignatureFactory;
import javax.xml.crypto.dsig.dom.DOMSignContext;
import javax.xml.crypto.dsig.dom.DOMValidateContext;
import javax.xml.crypto.dsig.keyinfo.KeyInfo;
import javax.xml.crypto.dsig.keyinfo.KeyInfoFactory;
import javax.xml.crypto.dsig.keyinfo.X509Data;
import javax.xml.crypto.dsig.spec.C14NMethodParameterSpec;
import javax.xml.crypto.dsig.spec.XSLTTransformParameterSpec;
import javax.xml.namespace.NamespaceContext;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathFactory;

import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.xml.sax.InputSource;

/**
 * Provides facilities for working with XML digital signatures and HealthVault things.
 */
public class ThingSignatureSample {

    /**
     *
     * Signs a HealthVault thing.
     *
     * @param thingXml The thing to sign as an XML string.
     * @param keyEntry A KeyStore.Entry containing the private key to use for signing.
     * 				   Must be a KeyStore.PrivateKeyEntry.
     * @return A string of the thingXml containing the digital signature.
     *
     */
    public String DigitallySign(String thingXml, KeyStore.Entry keyEntry)	throws Exception
    {
        // Load the document to sign.
        DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
        dbf.setNamespaceAware(true);
        InputSource source = new InputSource(new StringReader(thingXml));
        DocumentBuilder db = dbf.newDocumentBuilder();
        Document doc = db.parse(source);

        String signedThing = DigitallySignXml(doc, keyEntry);

        return signedThing;
    }

    private String DigitallySignXml(Document doc, KeyStore.Entry keyEntry)	throws Exception
    {
        if(!(keyEntry instanceof KeyStore.PrivateKeyEntry))
        {
            throw new Exception("keyEntry must be of type KeyStore.PrivateKeyEntry");

        }
        KeyStore.PrivateKeyEntry privateKeyEntry = (KeyStore.PrivateKeyEntry) keyEntry;
        Certificate cert = privateKeyEntry.getCertificate();

        if(!(cert instanceof X509Certificate))
        {
            throw new Exception("The key entry must have a corresponding X509Certificate.");
        }
        X509Certificate x509Cert =  (X509Certificate) cert;


        // Create the signature factory for creating the signature.
        XMLSignatureFactory sigFactory = XMLSignatureFactory.getInstance("DOM");

        List<Transform> transforms = new ArrayList<Transform>();

        // Create the XSLT for transforming the thing to be signed.
        DocumentBuilderFactory xsf = DocumentBuilderFactory.newInstance();
        xsf.setNamespaceAware(true);
        DocumentBuilder xsBuilder = xsf.newDocumentBuilder();
        InputSource is = new InputSource(new StringReader(SignatureV2XSLTString));
        Document xsltDoc = xsBuilder.parse(is);
        DOMStructure stylesheet = new DOMStructure(xsltDoc.getFirstChild());
        XSLTTransformParameterSpec spec = new XSLTTransformParameterSpec(stylesheet);
        Transform xslt = sigFactory.newTransform(Transform.XSLT, spec);
        transforms.add(xslt);

        // Create the canonicalization transform to be applied after the XSLT.
        Transform c14n = sigFactory.newCanonicalizationMethod(
                CanonicalizationMethod.INCLUSIVE, (C14NMethodParameterSpec) null);
        transforms.add(c14n);

        // Create the Reference to the XML to be signed specifying the hash algorithm to be used
        // and the list of transforms to apply. Also specify the XML to be signed as the current
        // document (specified by the first parameter being an empty string).
        Reference reference = sigFactory.newReference(
                "",
                sigFactory.newDigestMethod(DigestMethod.SHA1, null),
                transforms,
                null,
                null);

        // Create the Signed Info node of the signature by specifying the canonicalization method
        // to use (INCLUSIVE), the signing method (RSA_SHA1), and the Reference node to be signed.
        SignedInfo si = sigFactory.newSignedInfo(
                sigFactory.newCanonicalizationMethod(
                        CanonicalizationMethod.INCLUSIVE,
                        (C14NMethodParameterSpec) null),
                        sigFactory.newSignatureMethod(SignatureMethod.RSA_SHA1, null),
                        Collections.singletonList(reference));

        // Create the KeyInfo node containing the public key information to include in the signature.
        KeyInfoFactory kif = sigFactory.getKeyInfoFactory();
        X509Data xd = kif.newX509Data(Collections.singletonList(x509Cert));
        KeyInfo ki = kif.newKeyInfo(Collections.singletonList(xd));

        // Get the node to attach the signature.
        XPath xpath = XPathFactory.newInstance().newXPath();
        Node signatureInfoNode = (Node) xpath.evaluate("thing/signature-info", doc, XPathConstants.NODE);

        // Create a signing context using the private key.
        DOMSignContext dsc = new DOMSignContext(
                privateKeyEntry.getPrivateKey(),
                signatureInfoNode);

        // Create the signature from the signing context and key info
        XMLSignature signature = sigFactory.newXMLSignature(si, ki);
        signature.sign(dsc);

        // return the signed thing as string.
        StringWriter sw = new StringWriter();
        TransformerFactory tf = TransformerFactory.newInstance();
        Transformer trans = tf.newTransformer();
        trans.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
        trans.transform(new DOMSource(doc), new StreamResult(sw));

        String signedXml = sw.toString();

        return signedXml;
    }

    /**
     * Gets a PrivateKeyEntry that can be used for signing.
     *
     * @param keyStoreFilePath The path on the file system to a PKCS12 keystore containing the key entry.
     * @param keyStorePassword The password to use for reading the keystore.
     * @param keyEntryAlias	   The alias of the key entry within the keystore to read.
     * @param keyEntryPassword The password for reading the key entry.
     *
     * @return A KeyStore.PrivateKeyEntry containing a public and private key-pair.
     */
    public KeyStore.PrivateKeyEntry GetPrivateKeyEntry(
            String keyStoreFilePath,
            String keyStorePassword,
            String keyEntryAlias,
            String keyEntryPassword)
    throws KeyStoreException, NoSuchAlgorithmException, CertificateException, FileNotFoundException,
    IOException, UnrecoverableEntryException
    {
        KeyStore ks = KeyStore.getInstance("PKCS12");
        ks.load(
                new FileInputStream(keyStoreFilePath),
                keyStorePassword.toCharArray());
        KeyStore.PrivateKeyEntry keyEntry = (KeyStore.PrivateKeyEntry) ks.getEntry(
                keyEntryAlias,
                new KeyStore.PasswordProtection(keyEntryPassword.toCharArray()));


        return keyEntry;
    }

    public void ValidateSignature(Node thing) throws Exception
    {
        // Create a document out of the thing
        DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
        dbf.setIgnoringElementContentWhitespace(false);
        DocumentBuilder builder = dbf.newDocumentBuilder();
        Document doc = builder.newDocument();
        Node newThingNode = doc.importNode(thing, true);
        doc.appendChild(newThingNode);

        // Get the Signature node.
        XPath xpath = XPathFactory.newInstance().newXPath();
        xpath.setNamespaceContext(new SignatureNamespaceContext());
        Node signatureNode = (Node) xpath.evaluate("thing/signature-info/ds:Signature", doc, XPathConstants.NODE);
        DOMValidateContext valContext = new DOMValidateContext(new X509KeySelector(), signatureNode);

        // Create an signature factory and unmarshal the signature from XML.
        XMLSignatureFactory factory = XMLSignatureFactory.getInstance("DOM");
        XMLSignature signature = factory.unmarshalXMLSignature(valContext);

        // Validate the signature.
        boolean coreValidity = signature.validate(valContext);
        if(!coreValidity)
        {
            // log and throw if not valid.

            boolean sv = signature.getSignatureValue().validate(valContext);

            String msg = "signature validation status: " + sv;

            Iterator i = signature.getSignedInfo().getReferences().iterator();
            for (int j=0; i.hasNext(); j++) {
                boolean refValid = ((Reference)
                        i.next()).validate(valContext);
                msg += " ref["+j+"] validity status: " + refValid;
            }

            throw new Exception(msg);
        }
    }

    public class SignatureNamespaceContext implements NamespaceContext
    {
        public String getNamespaceURI(String prefix)
        {
            if (prefix.equals("ds"))
                return XMLSignature.XMLNS;
            else
                return XMLConstants.NULL_NS_URI;
        }

        public String getPrefix(String namespace)
        {
            if (namespace.equals(XMLSignature.XMLNS))
                return "ds";
            else
                return null;
        }

        public Iterator getPrefixes(String namespace)
        {
            return null;
        }
    }

    public static class X509KeySelector extends KeySelector {
        public KeySelectorResult select(KeyInfo keyInfo,
                KeySelector.Purpose purpose,
                AlgorithmMethod method,
                XMLCryptoContext context)
        throws KeySelectorException {
            Iterator ki = keyInfo.getContent().iterator();
            while (ki.hasNext()) {
                XMLStructure info = (XMLStructure) ki.next();
                if (!(info instanceof X509Data))
                    continue;
                X509Data x509Data = (X509Data) info;
                Iterator xi = x509Data.getContent().iterator();
                while (xi.hasNext()) {
                    Object o = xi.next();
                    if (!(o instanceof X509Certificate))
                        continue;
                    final PublicKey key = ((X509Certificate)o).getPublicKey();

                    if (algEquals(method.getAlgorithm(), key.getAlgorithm())) {
                        return new KeySelectorResult() {
                            public Key getKey() { return key; }
                        };
                    }
                }
            }
            throw new KeySelectorException("No key found!");
        }

        static boolean algEquals(String algURI, String algName) {
            if ((algName.equalsIgnoreCase("DSA") &&
                    algURI.equalsIgnoreCase(SignatureMethod.DSA_SHA1)) ||
                    (algName.equalsIgnoreCase("RSA") &&
                            algURI.equalsIgnoreCase(SignatureMethod.RSA_SHA1))) {
                return true;
            } else {
                return false;
            }
        }
    }

    private static final String SignatureV2XSLTString =
        "<xs:stylesheet xmlns:xs='http://www.w3.org/1999/XSL/Transform' version='1.0'>" +
            "<xs:template match='thing'>" +
                "<hv:signed-thing-data xmlns:hv='urn:com.microsoft.wc.thing.signing.2.xsl'>" +
                    "<xs:copy-of select='data-xml'/>" +
                    "<xs:copy-of select='signature-info/sig-data'/>" +
                "</hv:signed-thing-data>" +
            "</xs:template>" +
        "</xs:stylesheet>";
}
